using ecommerce.Data;
using FluentValidation;
using ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ecommerce.Validators
{
    public class RegisterUserValidator : AbstractValidator<RegisterUser>
    {
        private readonly UserManager<User> _userManager;

        public RegisterUserValidator(UserManager<User> userManager)
        {
            _userManager = userManager;

            RuleFor(user => user.UserName)
                .NotEmpty()
                .MinimumLength(3);

            RuleFor(user => user.Password)
                .NotEmpty()
                .MinimumLength(6);

            RuleFor(user => user.PasswordConfirm)
                .Equal(user => user.Password);

            RuleFor(user => user.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MustAsync(UniqueEmail)
                .WithMessage("Email already exists");
        }

        private async Task<bool> UniqueEmail(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user == null;
        }
    }
}
