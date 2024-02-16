using ecommerce.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ecommerce.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/roles")]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IdentityRole>>> GetRoles(int page = 1, int pageSize = 10)
        {
            var roles = _roleManager.Roles.AsQueryable();

            var paginatedRoles = await PaginatedList<IdentityRole>.CreateAsync(roles, page, pageSize);

            return Ok(paginatedRoles);
        }

        [HttpPost]
        public async Task<ActionResult> CreatePost(string Name)
        {
            if (!await _roleManager.RoleExistsAsync(Name))
            {
                var role = await _roleManager.CreateAsync(new IdentityRole { Name = Name });
                return CreatedAtAction(nameof(IdentityRole), new { id = role });
            }
            return BadRequest("already exisists");
        }
    }
}
