using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using ecommerce.Data;
using ecommerce.Dtos;
using ecommerce.Emails;
using ecommerce.Interfaces;
using ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;

namespace ecommerce.Helpers
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager,
            IConfiguration configuration, AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> RegisterUser(RegisterUser user)
        {
            var identityUser = new User
            {
                UserName = user.UserName,
                Email = user.Email,
                TwoFactorEnabled = true
            };

            var result = await _userManager.CreateAsync(identityUser, user.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);



                return new OkResult();
            }
            else
            {
                var errors = result.Errors.Select(error => error.Description).ToList();
                return new BadRequestObjectResult(new { Errors = errors });
            }
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return new BadRequestResult();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BadRequestResult();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return new OkResult();

            return new BadRequestResult();
        }

        public async Task<UserWithPermissionsDTO?> GetUserDetails()
        {
            var claimsIdentity = _httpContextAccessor.HttpContext.User.Identity;
            var emailClaim = ((ClaimsIdentity)claimsIdentity).FindFirst(ClaimTypes.Email)?.Value;

            if (emailClaim != null)
            {
                var user = await _userManager.FindByEmailAsync(emailClaim);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    var permissions = new HashSet<string>();
                    foreach (var roleName in roles)
                    {
                        var role = await _roleManager.FindByNameAsync(roleName);
                        if (role != null)
                        {
                            var rolePermissions = await _dbContext.RolePermission
                                .Where(rp => rp.RoleId == role.Id)
                                .Select(rp => rp.Permission.Name)
                                .ToListAsync();

                            permissions.AddRange(rolePermissions);
                        }
                    }

                    return new UserWithPermissionsDTO
                    {
                        User = new UserDTO
                        {
                            Name = user.UserName,
                            Email = user.Email,
                            IsEmailConfirmed = user.EmailConfirmed
                        },
                        Permissions = permissions.ToList()
                    };
                }
            }
            return null;
        }

        public async Task<string?> GenerateJwtToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && user.EmailConfirmed)
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, email),
                };

                var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                    _configuration["Jwt:Issuer"],
                    claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            return null;
        }

        public async Task<bool> CheckUserCredentials(LoginUser loginuser)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginuser.Email);

                if (user is null || !user.EmailConfirmed)
                    return false;

                var signInResult = await _userManager.CheckPasswordAsync(user, loginuser.Password);
                return signInResult;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
