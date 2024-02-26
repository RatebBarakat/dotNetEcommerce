using ecommerce.Emails;
using ecommerce.Filters;
using ecommerce.Interfaces;
using ecommerce.Models;
using FluentValidation;
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


        public AuthController(IAuthService authService, IValidator<LoginUser> loginValidator, IValidator<RegisterUser> registerValidator,
            IAuthenticationSchemeProvider authenticationSchemeProvider, SignInManager<User> signinManager, UserManager<User> userManager, SendEmailVerificationLink emailSender)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _signinManager = signinManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var data = await _userManager.ConfirmEmailAsync(user, code);
            return Ok(data.Succeeded);
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

            var mail = _emailSender.SendAsync(user.Email, confirmationUrl);

            return Ok(mail.Result);
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

        [HttpGet("google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            var info = await _signinManager.GetExternalLoginInfoAsync();
            if (info is null)
            {
                return BadRequest("An error occurred");
            }

            var result = await _signinManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

            if (result.Succeeded)
            {
                await _signinManager.UpdateExternalAuthenticationTokensAsync(info);
                return Redirect(returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return BadRequest("User creation failed");
                    }
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    return BadRequest("Adding external login failed");
                }

                await _signinManager.SignInAsync(user, isPersistent: false);

                return Redirect(returnUrl);
            }
        }

        [AllowAnonymous]
        [HttpGet("google/redirect")]
        public IActionResult ExternalLogin(string provider)
        {
            var returnUrl = Url.Content("~/");
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { returnUrl = returnUrl});
            var properties = _signinManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }

        [HttpPost("logout")]

        public IActionResult logout ()
        {
            Response.Cookies.Delete("seid");
            return Ok();
        }
    }
}
