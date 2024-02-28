using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce.Models;
using ecommerce.Data;
using Microsoft.AspNetCore.Authorization;
using ecommerce.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using ecommerce.Attributes;
using ecommerce.Excel;
using FluentValidation;
using ecommerce.Dtos;
using ecommerce.Validators;

namespace ecommerce.Controllers.Users
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IRedis _redis;

        public CategoryController(AppDbContext context, IRedis redis)
        {
            _context = context;
            _redis = redis;
        }


        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<Category>>> GetProductsByCategoryId([FromQuery] int page, int id)
        {
            var products = _context.Products
                .Where(p => p.CategoryId == id)
                .Include(p => p.Category)
                .Include(c => c.Images)
                .AsQueryable();

            string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var paginatedProducts = await PaginatedList<Product>.CreateAsync(products, page, 10);

            var data = paginatedProducts.data.Select(r => r.ToDto(baseUrl)).ToList();
            var result = new PaginatedList<ProductDTO>(data, paginatedProducts.total, paginatedProducts.page, 10);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            IEnumerable<Category> categories;
            var categoriesFromCache = await _redis.GetCachedDataAsync<IEnumerable<Category>>("categories");
            if (categoriesFromCache is not null)
            {
                categories = categoriesFromCache;
            }
            else
            {
                categories = await _context.Categories.ToListAsync();
                await _redis.SetCachedDataAsync("categories", categories, new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(1),
                });
            }
            return Ok(categories);
        }
    }
}
