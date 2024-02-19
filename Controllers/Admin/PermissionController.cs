using ecommerce.Data;
using ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/permissions")]
    public class PermissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermissionController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permission>>> Index ()
        {
            var permissions = await _context.Permissions.ToListAsync();

            return Ok(permissions);
        }

        [HttpPost]
        public async Task<ActionResult<Permission>> CreateCategory(string name)
        {
            var permission = new Permission
            {
                Id = Guid.NewGuid().ToString(),
                Name = name
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateCategory), new { name = permission.Name }, permission);
        }


        [HttpPut]
        public async Task<ActionResult<IEnumerable<Permission>>> Update(string id, string name)
        {
            var permission = await _context.Permissions.FindAsync(id);

            if (permission is null)
            {
                return BadRequest();
            }

            permission.Name = name;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult<IEnumerable<Permission>>> Delete(string id)
        {
            var permission = await _context.Permissions.FindAsync(id);

            if (permission is null)
            {
                return BadRequest();
            }

            _context.Permissions.Remove(permission);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
