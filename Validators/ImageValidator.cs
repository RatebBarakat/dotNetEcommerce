using FluentValidation;

namespace ecommerce.Validators
{
    public class ImageValidator : AbstractValidator<IFormFile>
    {
        public ImageValidator()
        {
            RuleFor(x => x.ContentType).NotNull().WithMessage("here is the error").Must(x => x.Equals("image/jpeg") || x.Equals("image/jpg") || x.Equals("image/png"))
                .WithMessage("File type is not allowed");
        }
    }
}
