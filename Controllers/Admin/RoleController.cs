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
                    Id = r.Id,
                    Name = r.Name,
                    Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList()
                })
                .AsQueryable();

            var paginatedRoles = await PaginatedList<object>.CreateAsync(roles, page, pageSize);

            return Ok(paginatedRoles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRoles(string id)
        {
            var role = await _context.Roles.Where(r => r.Id == id).FirstOrDefaultAsync();
            return Ok(role);
        }

        [HttpGet("{id}/permissions")]
        public async Task<ActionResult> GetPermissionsByRoleId(string id)
        {
            var permissions = await _context.Roles
                .Where(r => r.Id == id)
                .SelectMany(r => r.RolePermissions)
                .Select(rp => new { rp.Permission.Id, rp.Permission.Name })
                .ToListAsync();

            return Ok(permissions);
        }

        [HttpPost]
        public async Task<ActionResult> CreateRole(Role role)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name))
            {
                var roleCreated = await _roleManager.CreateAsync(new Role { Name = role.Name });
                return CreatedAtAction("CreateRole", new { id = role });
            }
            return BadRequest("already exisists");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRole(string id, Role role)
        {
            var roleEntry = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == id);

            if (roleEntry is null)
            {
                return BadRequest("role not found");
            }

            roleEntry.Name = role.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(string id)
        {
            var role = _roleManager.Roles.FirstOrDefault(r => r.Id == id);
            if (role is null)
            {
                return BadRequest("role not found");
            }
            await _roleManager.DeleteAsync(role);
            return NoContent();
        }

        [HttpPut("{id}/sync")]
        public async Task<ActionResult> SyncPermissions(string id, List<string> permissionsIds)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);

            if (role is null)
            {
                return BadRequest("Role not found");
            }

            var existingRolePermissions = await _context.RolePermission.Where(rp => rp.RoleId == id).ToListAsync();
            _context.RolePermission.RemoveRange(existingRolePermissions);

            foreach (var permId in permissionsIds)
            {
                var permissionEntry = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permId);

                if (permissionEntry != null)
                {
                    role.RolePermissions.Add(new RolePermission { PermissionId = permissionEntry.Id });
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}
