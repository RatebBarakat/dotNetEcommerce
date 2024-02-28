using ecommerce.Data;
using ecommerce.Dtos;
using ecommerce.Models;
using ecommerce.Services;
using ecommerce.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Controllers.Admin
{
    [ApiController]
    [Authorize(Policy = "EmailConfirmedPolicy")]
    [Route("api/admin/images")]
    public class ImagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ImageHelper _imageHelper;
        private readonly ImageValidator _validator;

        public ImagesController(AppDbContext context, ImageHelper imageHelper, ImageValidator validator)
        {
            _context = context;
            _imageHelper = imageHelper;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> InsertImage(ImageDto model)
        {
            foreach (var image in model.Images)
            {
                var validationResult = _validator.Validate(image);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "errors",
                        Errors = validationResult.Errors.ToDictionary(
                            e => "Image",
                            e => e.ErrorMessage
                        )
                    });
                }
            }
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == model.ProductId);

            if (product is null)
            {
                return BadRequest(new { message = "product not found" });
            }

            if ((product.Images.Count + model.Images.Count) > 4)
            {
                return BadRequest(new { message = $"you cant upload more than  images" });
            }

            foreach (var image in model.Images)
            {
                var fileName = await _imageHelper.UploadImage(image);
                product.Images.Add(new ProductImages
                {
                    Name = fileName,
                });
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteImage(DeleteImageDto model)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == model.ProductId);

            if (product is null)
            {
                return BadRequest(new { message = "product not found" });
            }

            if (product.Images.Count > 4)
            {
                return BadRequest(new { message = "you cant upload more than  images" });
            }

            var productImage = await _context.ProductImages.Where(p => p.Id == model.ImageId && p.ProductId == model.ProductId).FirstOrDefaultAsync();

            if (productImage is null)
            {
                return BadRequest(new { message = "image not found" });
            }

            product.Images.Remove(productImage);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
