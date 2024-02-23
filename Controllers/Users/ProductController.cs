using ecommerce.Data;
using ecommerce.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Controllers.Users
{
    [Route("api/user/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("home")]
        public async Task<IActionResult> GetHomePageData()
        {
            Random rand = new Random();
            var count = _context.Categories.Count();
            int skipper = count > 10 ? rand.Next(0, count - 5) : 0;
            string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            var categories = await _context.Categories
                .Include(c => c.Products)
                .ThenInclude(p => p.Images)
                .Where(p => p.Products.Any())
                .OrderBy(product => Guid.NewGuid())
                .Skip(skipper)
                .Take(5)
                .Select(c => new
                {
                    Category = new
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
                        })
                    },
                })
                .ToListAsync();

            return Ok(categories);
        }
    }
}
