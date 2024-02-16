using ecommerce.Data;
using ecommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaginatedList<User>>>> GetUsers()
        {
            var users = _context.Users.AsQueryable();
            int page;
            int.TryParse(HttpContext.Request.Query["page"].ToString(), out page);
            var paginatedCategories = await PaginatedList<User>.CreateAsync(users, page, 10);
            return Ok(paginatedCategories); 
        }
    }
}
