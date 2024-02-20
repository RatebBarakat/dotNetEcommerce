using ecommerce.Dtos;
using FluentValidation;

namespace ecommerce.Validators
{
    public class ExcelValidator : AbstractValidator<IFormFile>
    {
        public ExcelValidator()
        {
            RuleFor(x => x.ContentType)
                .NotNull().WithMessage("Content type cannot be null")
                .Must(x => x.Equals("application/vnd.ms-excel") || x.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                .WithMessage("File type is not allowed");

            RuleFor(x => x.FileName)
                .NotNull().WithMessage("File name cannot be null")
                .Must(x => x.EndsWith(".xls") || x.EndsWith(".xlsx"))
                .WithMessage("File extension is not allowed");
        }

    }
}
