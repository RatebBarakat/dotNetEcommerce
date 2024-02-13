using ecommerce.Filters;
using ecommerce.Interfaces;
using ecommerce.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using static ecommerce.Filters.GuestOnly;

namespace ecommerce.Controllers.Auth
{
    [ApiController]
    [Route("api/user")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterUser> _registerValidator;
        private readonly IValidator<LoginUser> _loginValidator;

        public AuthController(IAuthService authService, IValidator<LoginUser> loginValidator, IValidator<RegisterUser> registerValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        [HttpPost("register")]
        [TypeFilter(typeof(GuestOnly))]
        public async Task<IActionResult> Register([FromBody] RegisterUser user)
        {
            var validationResult = await _registerValidator.ValidateAsync(user);

            if (!validationResult.IsValid)
            {
                return BadRequest(new { Message = "Registration failed. Please check the provided information.", Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var result = await _authService.RegisterUser(user);

            if (result is OkResult)
            {
                return Ok("User registered successfully.");
            }
            else if (result is BadRequestObjectResult badRequestResult)
            {
                var errors = badRequestResult.Value as List<string>;
                return BadRequest(new { Message = "Registration failed. Please check the provided information.", Errors = badRequestResult.Value });
            }

            return StatusCode(500, "An unexpected error occurred.");
        }

        [HttpGet("info")]
        public async Task<IActionResult> getUserDetails()
        {
            var user = await _authService.GetUserDetails();
            if (user is null)
            {
                return BadRequest();
            }
            return Ok(new
            {
                user = user
            });
        }

        [HttpPost("login")]
        [TypeFilter(typeof(GuestOnly))]
        public async Task<IActionResult> Login([FromBody] LoginUser user)
        {
            var validationResult = _loginValidator.Validate(user);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = "Registration failed. Please check the provided information.",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            if (await _authService.CheckUserCredentials(user))
            {
                var token = _authService.GenerateJwtToken(user.Email);

                Response.Cookies.Append("seid", token, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddHours(1)
                });

                return Ok(new { Message = "you're logged in successfully", Token = token });
            }
            return BadRequest(new { Errors = "invalid login credentials" });
        }        
    }
}
