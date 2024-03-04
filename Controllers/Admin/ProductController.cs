using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce.Models;
using ecommerce.Data;
using ecommerce.Dtos;
using FluentValidation;
using ecommerce.Excel;
using ecommerce.Validators;
using ecommerce.Services;
using Hangfire;
using ecommerce.Services.Excel;
using ecommerce.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ProductExcelImportService _excelService;

        public ProductController(AppDbContext context,
            IValidator<CreateProductDTO> productValidator, ExcelValidator excelValidator, ImageHelper imageHelper, IWebHostEnvironment webHostEnvironment, ProductExcelImportService excelService)
        {
            _context = context;
            _productValidator = productValidator;
            _excelValidator = excelValidator;
            _imageHelper = imageHelper;
            _webHostEnvironment = webHostEnvironment;
            _excelService = excelService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaginatedList<ProductDTO>>>> GetProducts(int page = 1, int pageSize = 50,
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

        [HttpPost("import")]
        public IActionResult ImportFromExcel(IFormFile file)
        {
            try
            {
                var status = _excelValidator.Validate(file);
                if (!status.IsValid)
                {
                    return BadRequest("invalid file");
                }

                int size = 100;
                var fileName = SaveFile(file);
                var claimsIdentity = HttpContext.User.Identity;
                var emailClaim = ((ClaimsIdentity)claimsIdentity).FindFirst(ClaimTypes.Email)?.Value;

                BackgroundJob.Enqueue(() => _excelService.Start(fileName, 100,emailClaim));

                return Ok("excel file is being imported in the background");
            }
            catch (Exception e)
            {
                return BadRequest($"an error occurred {e.Message} {e.InnerException}");
            }
        }

        private string SaveFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "excel");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return filePath;
        }
    }
}
