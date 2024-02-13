﻿using Azure;
using Azure.Core;
using ecommerce.Data;
using ecommerce.Interfaces;
using ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ecommerce.Hepers
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<User> _loginManager;
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(UserManager<User> userManager, SignInManager<User> loginManager,
            IConfiguration configuration, AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _loginManager = loginManager;
            _configuration = configuration;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> RegisterUser(RegisterUser user)
        {
            var identityUser = new User
            {
                UserName = user.UserName,
                Email = user.Email,
            };

            var result = await _userManager.CreateAsync(identityUser, user.Password);

            if (result.Succeeded)
            {
                return new OkResult();
            }
            else
            {
                var errors = result.Errors.Select(error => error.Description).ToList();

                return new BadRequestObjectResult(new { Errors = errors });
            }
        }

        public async Task<object?> GetUserDetails()
        {
            var claimsIdentity = _httpContextAccessor.HttpContext.User.Identity;
            var emailClaim = ((ClaimsIdentity)claimsIdentity).FindFirst(ClaimTypes.Email)?.Value;

            if (emailClaim != null)
            {
                var data =  await _userManager.FindByEmailAsync(emailClaim);
                return new
                {
                    name = data.UserName,
                    email = data.Email,
                    is_email_confirmed = data.EmailConfirmed
                };
            }

            return null;
        }

        public string GenerateJwtToken(string email)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email)
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
             );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> CheckUserCredentials(LoginUser loginuser)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginuser.Email);

                if (user is null)
                    return false;

                var passwordValid = await _userManager.CheckPasswordAsync(user, loginuser.Password);

                return passwordValid;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
