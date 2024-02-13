using ecommerce.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ecommerce.Validators
{
    public class LoginUserValidator : AbstractValidator<LoginUser>
    {
        private readonly UserManager<User> _userManager;

        public LoginUserValidator(UserManager<User> userManager)
        {
            _userManager = userManager;

            RuleFor(user => user.Password)
                .NotEmpty()
                .MinimumLength(6);

            RuleFor(user => user.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
