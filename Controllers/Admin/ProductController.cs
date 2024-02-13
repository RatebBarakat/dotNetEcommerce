﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce.Models;
using ecommerce.Data;
using ecommerce.Dtos;

namespace ecommerce.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/products")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var Products = await _context.Products.ToListAsync();
            return Ok(Products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromForm] ProductDTO productDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var imageFileName = await UploadImage(productDTO.Image);

            var product = new Product
            {
                Name = productDTO.Name,
                Quantity = productDTO.Quantity,
                Price = productDTO.Price,
                SmallDescription = productDTO.SmallDescription,
                Description = productDTO.Description,
                Image = imageFileName,
                CategoryId = productDTO.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDTO productDTO)
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

            if (!string.IsNullOrEmpty(existingProduct.Image))
            {
                await DeleteImage(existingProduct.Image);
            }

            var newImageFileName = await UploadImage(productDTO.Image);

            existingProduct.Name = productDTO.Name;
            existingProduct.Quantity = productDTO.Quantity;
            existingProduct.Price = productDTO.Price;
            existingProduct.SmallDescription = productDTO.SmallDescription;
            existingProduct.Description = productDTO.Description;
            existingProduct.Image = newImageFileName;
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

            if (System.IO.File.Exists(name))
            {
                System.IO.File.Delete(name);
                return true;
            }
            return false;
        }
    }
}