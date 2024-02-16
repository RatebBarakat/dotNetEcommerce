using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce.Models;
using ecommerce.Data;
using Microsoft.AspNetCore.Authorization;
using ecommerce.Interfaces;
using System.Collections;
using Microsoft.Extensions.Caching.Distributed;

namespace ecommerce.Controllers.Admin
{
    [ApiController]
    [Authorize(Policy = "EmailConfirmedPolicy")]
    [Route("api/admin/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IRedis _redis;

        public CategoryController(AppDbContext context,IRedis redis)
        {
            _context = context;
            _redis = redis;
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<PaginatedList<Category>>>> GetCategories()
        {
            var categories = _context.Categories.AsQueryable();
            int page;
            int.TryParse(HttpContext.Request.Query["page"].ToString(),out page);
            var paginatedCategories = await PaginatedList<Category>.CreateAsync(categories,page,10);
            return Ok(paginatedCategories);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            IEnumerable<Category> categories;
            var categoriesFromCache = await _redis.GetCachedDataAsync<IEnumerable<Category>>("categories");
            if (categoriesFromCache is not null)
            {
                categories = categoriesFromCache;
            } else
            {
                categories = await _context.Categories.ToListAsync();
                await _redis.SetCachedDataAsync("categories",categories, new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(1),
                });
            }
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(c => c.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
