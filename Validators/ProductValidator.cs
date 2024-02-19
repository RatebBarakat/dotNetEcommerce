﻿using ecommerce.Data;
using ecommerce.Dtos;
using ecommerce.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Validators
{
    public class ProductValidator : AbstractValidator<CreateProductDTO>
    {
        private readonly AppDbContext _context;

        public ProductValidator(AppDbContext appDbContext)
        {
            _context = appDbContext;
            RuleFor(p => p.Name).NotEmpty().MustAsync(UniqueName);
            RuleFor(p => p.SmallDescription).MinimumLength(10).MaximumLength(100);
            RuleFor(p => p.Description).MinimumLength(100).MaximumLength(1000);
            RuleFor(p => p.Price).InclusiveBetween(0, int.MaxValue);
            RuleFor(p => p.Quantity).InclusiveBetween(0, 1000);
            RuleFor(p => p.CategoryId).MustAsync(CategoryExists).WithMessage("category doeasnt exists");
            RuleFor(p => p.Images).Must(x => x.Count > 0).WithMessage("ypu should upload at least one image")
                .Must(x => x.Count <= 5).WithMessage("ypu cant upload more than 5 images");
            RuleForEach(p => p.Images).SetValidator(new ImageValidator());
        }

        private async Task<bool> CategoryExists(int id, CancellationToken cancellationToken)
        {
            var category = await _context.Categories.AnyAsync(c => c.Id == id);
            return category != null;
        }

        private async Task<bool> UniqueName(string name, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == name);
            return product == null;
        }
    }
}
