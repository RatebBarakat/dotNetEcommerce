using ecommerce.Data;
using ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ecommerce.Helpers
{
    public class PermissionHelper
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public PermissionHelper(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> HasPermission(string email, string permission)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return false;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in userRoles)
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Name == roleName);

                if (role != null && role.RolePermissions.Any(rp => rp.Permission.Name == permission))
                {
                    return true; 
                }
            }

            return false;
        }

    }
}
