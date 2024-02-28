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

namespace ecommerce.Controllers.Admin
{
    [ApiController]
    [Authorize(Policy = "EmailConfirmedPolicy")]
    [Route("api/admin/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ExcelValidator _excelValidator;
        private readonly IRedis _redis;

        public CategoryController(AppDbContext context, ExcelValidator excelValidator)
        {
            _context = context;
            _excelValidator = excelValidator;
        }

        [HttpGet]
        [HasPermissions("Permission:manage-categories")]
        public async Task<ActionResult<PaginatedList<Category>>> GetCategories([FromQuery] int page = 1, [FromQuery] string search = "")
        {
            var categories = _context.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                categories = categories.Where(c => c.Name.ToLower().Contains(search.ToLower()));
            }

            var paginatedCategories = await PaginatedList<Category>.CreateAsync(categories, page, 10);
            return Ok(paginatedCategories);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            /*            IEnumerable<Category> categories;
                        var categoriesFromCache = await _redis.GetCachedDataAsync<IEnumerable<Category>>("categories");*/
            /*            if (categoriesFromCache is not null)
                        {
                            categories = categoriesFromCache;
                        }
                        else
                        {
                            await _redis.SetCachedDataAsync("categories", categories, new DistributedCacheEntryOptions
                            {
                                AbsoluteExpiration = DateTime.Now.AddHours(1),
                            });
                        }*/
            var categories = await _context.Categories.ToListAsync();

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
        [HasPermissions("Permission:create-categories")]

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

        [HttpPost("import")]
        public async Task<ActionResult<Category>> ImportFromExcel(IFormFile file)
        {
            try
            {
                var status = _excelValidator.Validate(file);
                if (!status.IsValid)
                {
                    return BadRequest("invalid file");
                }
                List<Category> categories = new();
                var excel = new ExcelImportService<Category>(file);
                var Count = excel.GetCountOfRows();
                for (int i = 2; i < Count; i++)
                {
                    try
                    {
                        string Name = excel.GetAttributeValue("Name", i);
                        if (string.IsNullOrWhiteSpace(Name))
                        {
                            continue;
                        }
                        _context.Categories.Add(
                            new Category
                            {
                                Name = Name
                            });
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                await _context.SaveChangesAsync();

                return Ok("categories imported successfully");
            }
            catch (Exception e)
            {
                return BadRequest("an error occure");
            }
        }

        [HasPermissions("Permission:update-categories")]
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

        [HasPermissions("Permission:delete-categories")]
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

        [HttpPost("deleteMany")]
        public async Task<IActionResult> DeleteManyProducts(DeleteManyDto model)
        {
            var categories = await _context.Categories.Where(p => model.ids.Contains(p.Id)).ToListAsync();
            if (categories == null)
            {
                return NotFound();
            }

            _context.Categories.RemoveRange(categories);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
