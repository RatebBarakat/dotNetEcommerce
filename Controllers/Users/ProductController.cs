using ecommerce.Data;
using ecommerce.Dtos;
using ecommerce.Interfaces;
using ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Controllers.Users
{
    [Route("api/user/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IRedis _redis;
        
        public ProductController(AppDbContext context, IRedis redis)
        {
            _context = context;
            _redis = redis;
        }

        [HttpGet("home")]
        public async Task<IActionResult> GetHomePageData()
        {
            Random rand = new Random();
            var count = _context.Categories.Count();
            int takeCount = count >= 5 ? 5 : count;
            int skipper = count > 10 ? rand.Next(0, count - takeCount) : 0;

            string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            var dataFromCache = await _redis.GetCachedDataAsync<IEnumerable<CategoryDto>>("HomePageRandomCategories");
            if (dataFromCache is not null)
            {
                return Ok(dataFromCache);
            }

            var categories = await _context.Categories
                .Include(c => c.Products)
                .ThenInclude(p => p.Images)
                .Where(p => p.Products.Any())
                .OrderBy(product => Guid.NewGuid())
                .Skip(skipper)
                .Take(5)
                .Select(c => new CategoryDto
                {
                    Name = c.Name,
                    Products = c.Products.Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Quantity = p.Quantity,
                        Image = p.Images.Count > 0 ?
                            $"{baseUrl}/uploads/images/{p.Images.FirstOrDefault().Name}"
                            : null
                    }).ToList()
                })
                .ToListAsync();

            _redis.SetCachedDataAsync<List<CategoryDto>>("HomePageRandomCategories", categories, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.UtcNow.AddDays(1),
                
            });

            return Ok(categories);
        }
    }
}
