using ecommerce.Data;
using ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/roles")]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly AppDbContext _context;
        public RoleController(RoleManager<Role> roleManager, AppDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRoles(int page = 1, int pageSize = 10)
        {
            var roles = _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Select(r => new
                {
                    Name = r.Name,
                    Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList()
                })
                .AsQueryable();

            var paginatedRoles = await PaginatedList<object>.CreateAsync(roles, page, pageSize);

            return Ok(paginatedRoles);
        }


        [HttpPost]
        public async Task<ActionResult> CreateRole(string Name)
        {
            if (!await _roleManager.RoleExistsAsync(Name))
            {
                var role = await _roleManager.CreateAsync(new Role { Name = Name });
                return CreatedAtAction("CreateRole", new { id = role });
            }
            return BadRequest("already exisists");
        }

        [HttpPut]
        public async Task<ActionResult> UpdateRole(string Id, string Name)
        {
            var role = _roleManager.Roles.FirstOrDefault(r => r.Id == Id);
            if (role is null)
            {
                return BadRequest("role not found");
            }
            await _roleManager.SetRoleNameAsync(role, Name);
            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteRole(string Id)
        {
            var role = _roleManager.Roles.FirstOrDefault(r => r.Id == Id);
            if (role is null)
            {
                return BadRequest("role not found");
            }
            await _roleManager.DeleteAsync(role);
            return NoContent();
        }

        [HttpPut("sync")]
        public async Task<ActionResult> SyncPermissions(string RoleId, List<string> permissions)
        {
            var role = await _context.Roles.FirstAsync(r => r.Id == RoleId);

            if (role is null)
            {
                return BadRequest("Role not found");
            }

            role.RolePermissions.Clear();

            foreach (var permission in permissions)
            {
                var permissionentry = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permission);

                if (permissionentry is null)
                {
                    continue;
                }
                role.RolePermissions.Add(new RolePermission { PermissionId = permissionentry.Id });
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
