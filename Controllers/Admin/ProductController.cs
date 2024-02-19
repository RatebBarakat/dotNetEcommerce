﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce.Models;
using ecommerce.Data;
using ecommerce.Dtos;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;

namespace ecommerce.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/products")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IValidator<CreateProductDTO> _productValidator;

        public ProductController(AppDbContext context, IWebHostEnvironment env, IValidator<CreateProductDTO> productValidator)
        {
            _context = context;
            _env = env;
            _productValidator = productValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaginatedList<ProductDTO>>>> GetProducts(int page = 1, int pageSize = 10, int categoryFilter = 0)
        {
            var products = _context.Products.Include(p => p.Category).Include(c => c.Images).AsQueryable();
            if (categoryFilter != 0)
            {
                products = products.Where(p => p.CategoryId == categoryFilter);
            }
            string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var paginatedProducts = await PaginatedList<Product>.CreateAsync(products, page, 10);
            var data = paginatedProducts.data.Select(r => r.ToDto(baseUrl)).ToList();
            var result = new PaginatedList<ProductDTO>(data, paginatedProducts.total, paginatedProducts.page, 10);
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
                return BadRequest(new { Message = "errors", Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
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
                var imageFileName = await UploadImage(image);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
    }
}
