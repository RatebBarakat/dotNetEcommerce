using ecommerce.Emails;
using ecommerce.Filters;
using ecommerce.Interfaces;
using ecommerce.Models;
using FluentValidation;
using Google.Apis.Auth;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ecommerce.Controllers.Auth
{
    [ApiController]
    [Route("api/user")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterUser> _registerValidator;
        private readonly IValidator<LoginUser> _loginValidator;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly SignInManager<User> _signinManager;
        private readonly UserManager<User> _userManager;
        private readonly SendEmailVerificationLink _emailSender;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AuthController(IAuthService authService, IValidator<LoginUser> loginValidator, IValidator<RegisterUser> registerValidator,
            IAuthenticationSchemeProvider authenticationSchemeProvider, SignInManager<User> signinManager, UserManager<User> userManager, SendEmailVerificationLink emailSender, IBackgroundJobClient backgroundJobClient)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _signinManager = signinManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var data = await _userManager.ConfirmEmailAsync(user, code);
            if (data.Succeeded)
            {
                return Redirect("http://localhost:5173/");
            }
            return BadRequest("can't confirm your email");
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

            var userData = await _userManager.FindByEmailAsync(user.Email);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(userData);

            var confirmationUrl = Url.Action("ConfirmEmail", "Auth", new { userId = userData.Id, code = token }, Request.Scheme);

            if (confirmationUrl == null)
            {
                return StatusCode(500, "Failed to generate confirmation URL.");
            }

            Task sendMail = new Task(() =>
            {
                _emailSender.SendAsync(user.Email, confirmationUrl);
            });

            sendMail.Start();

            string? accessToken = await _authService.GenerateJwtToken(user.Email);

            Response.Cookies.Append("seid", accessToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok("please confirm your email");
        }

        [HttpGet("info")]
        public async Task<IActionResult> getUserDetails()
        {
            var user = await _authService.GetUserDetails();

            if (user is null)
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
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
                string? token = await _authService.GenerateJwtToken(user.Email);

                if (token is null)
                {
                    return BadRequest("");
                }

                Response.Cookies.Append("seid", token, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddHours(1)
                });

                return Ok(new { Message = "you're logged in successfully", Token = token });
            }
            return BadRequest(new { Errors = "invalid login credentials" });
        }

        [HttpPost("google/login")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback([FromBody] string code)
        {
            GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { "451812955571-dajpm95u4r5kt9dmfla84d9u62c9g0ed.apps.googleusercontent.com" }
            };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(code, settings);

            var user = await _userManager.FindByEmailAsync(payload.Email);
            string? token = string.Empty;

            if (user is null)
            {
                await _authService.RegisterUser(new RegisterUser
                {
                    Email = payload.Email,
                    Password = "RandomPass@11$",//fake password till now
                    PasswordConfirm = "RandomPass@11$",
                    UserName = payload.GivenName,
                }, true);
            }

            token = await _authService.GenerateJwtToken(payload.Email);

            Response.Cookies.Append("seid", token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(1)
            });
            return Ok(payload);
        }

        [HttpPost("logout")]

        public IActionResult logout ()
        {
            Response.Cookies.Delete("seid");
            return Ok();
        }
    }
}
