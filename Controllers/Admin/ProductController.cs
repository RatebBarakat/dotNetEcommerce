using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce.Models;
using ecommerce.Data;
using ecommerce.Dtos;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using ecommerce.Excel;
using ecommerce.Validators;
using System.Text;
using System.Collections.Concurrent;
using ecommerce.Services;

namespace ecommerce.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/products")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ExcelValidator _excelValidator;
        private readonly IValidator<CreateProductDTO> _productValidator;
        private readonly ImageHelper _imageHelper;

        public ProductController(AppDbContext context,
            IValidator<CreateProductDTO> productValidator, ExcelValidator excelValidator, ImageHelper imageHelper)
        {
            _context = context;
            _productValidator = productValidator;
            _excelValidator = excelValidator;
            _imageHelper = imageHelper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaginatedList<ProductDTO>>>> GetProducts(int page = 1, int pageSize = 100,
            [FromQuery] int categoryFilter = 0, [FromQuery] string search = "")
        {
            var products = _context.Products.Include(p => p.Category).Include(c => c.Images).AsQueryable();
            if (categoryFilter != 0)
            {
                products = products.Where(p => p.CategoryId == categoryFilter);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p => p.Name.ToLower().Contains(search.ToLower()) || p.Description.ToLower().Contains(search.ToLower()));
            }

            string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var paginatedProducts = await PaginatedList<Product>.CreateAsync(products, page, pageSize);

            var data = paginatedProducts.data.Select(r => r.ToDto(baseUrl)).ToList();

            var result = new PaginatedList<ProductDTO>(data, paginatedProducts.total, paginatedProducts.page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UpdateProductDto>> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.Category).Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            return Ok(product.ToUpdateDto(baseUrl));
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(CreateProductDTO productDTO)
        {
            var validationResult = await _productValidator.ValidateAsync(productDTO);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = "errors",
                    Errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => e.ErrorMessage
                    )
                });
            }


            var product = new Product
            {
                Name = productDTO.Name,
                Quantity = productDTO.Quantity,
                Price = productDTO.Price,
                SmallDescription = productDTO.SmallDescription,
                Description = productDTO.Description,
                CategoryId = productDTO.CategoryId,
                Images = new List<ProductImages>()
            };

            foreach (var image in productDTO.Images)
            {
                var imageFileName = await _imageHelper.UploadImage(image);
                product.Images.Add(new ProductImages
                {
                    Name = imageFileName,
                });
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] CreateProductDTO productDTO)
        {
            var validationResult = await _productValidator.ValidateAsync(productDTO);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = "errors",
                    Errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => e.ErrorMessage
                    )
                });
            }

            Product existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            /*       if (!string.IsNullOrEmpty(existingProduct.Image))
                   {
                       await DeleteImage(existingProduct.Image);
                   }*/

            /*            var newImageFileName = await UploadImage(productDTO.Image);
            */
            existingProduct.Name = productDTO.Name;
            existingProduct.Quantity = productDTO.Quantity;
            existingProduct.Price = productDTO.Price;
            existingProduct.SmallDescription = productDTO.SmallDescription;
            existingProduct.Description = productDTO.Description;
            existingProduct.CategoryId = productDTO.CategoryId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = "an error occured" });
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var Product = await _context.Products.FindAsync(id);
            if (Product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(Product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("deleteMany")]
        public async Task<IActionResult> DeleteManyProducts(DeleteManyDto model)
        {
            var Products = await _context.Products.Where(p => model.ids.Contains(p.Id)).ToListAsync();
            if (Products == null)
            {
                return NotFound();
            }

            _context.Products.RemoveRange(Products);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<string> UploadImage(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", @"uploads/images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        private async Task<bool> DeleteImage(string name)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", @"uploads/images");

            var filePath = Path.Combine(uploadsFolder, name);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return true;
            }
            return false;
        }

        [HttpPost("import")]
        public async Task<ActionResult<Product>> ImportFromExcel(IFormFile file)
        {
            try
            {
                var status = _excelValidator.Validate(file);
                if (!status.IsValid)
                {
                    return BadRequest("Invalid file");
                }

                var excel = new ExcelImportService<Product>(file);
                var count = excel.GetCountOfRows();
                var batchSize = 50;

                var errorsRows = new ConcurrentDictionary<int, string>();
                var tasks = new List<Task>();

                for (int i = 2; i < count + 2; i += batchSize)
                {
                    int start = i;
                    int end = Math.Min(i + batchSize, count + 2);

                    tasks.Add(Task.Run(async () =>
                    {
                        for (int j = start; j < end; j++)
                        {
                            try
                            {
                                string? name = excel.GetAttributeValue("Name", j);
                                int quantity;
                                int.TryParse(excel.GetAttributeValue("Quantity", j), out quantity);
                                decimal price;
                                decimal.TryParse(excel.GetAttributeValue("Price", j).Trim(), out price);
                                string? smallDescription = excel.GetAttributeValue("SmallDescription", j);
                                string? description = excel.GetAttributeValue("Description", j);
                                string? categoryName = excel.GetAttributeValue("Category", j);
                                int categoryId = await GetCategoryId(categoryName);
                                errorsRows.TryAdd(j, price.ToString());

                                _context.Products.Add(new Product
                                {
                                    Name = name,
                                    Quantity = quantity,
                                    Price = price,
                                    SmallDescription = smallDescription ?? "",
                                    Description = description ?? "",
                                    CategoryId = categoryId
                                });
                            }
                            catch (Exception ex)
                            {
                                errorsRows.TryAdd(j, ex.Message);
                            }
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                       
                await _context.SaveChangesAsync();

                return Ok(errorsRows.IsEmpty ? "Products imported successfully" : $"An error occurred in rows: {string.Join(",", errorsRows.Keys)}");
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred: {e.Message}");
            }
        }


        private async Task<int> GetCategoryId(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name of category is required");
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);

            if (category != null)
            {
                return category.Id;
            }
            else
            {
                var newCat = await _context.Categories.AddAsync(new Category
                {
                    Name = name
                });

                await _context.SaveChangesAsync();

                return newCat.Entity.Id;
            }
        }


    }
}
