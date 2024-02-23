using ecommerce.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string SmallDescription { get; set; }
        public string Description { get; set; }
        [ForeignKey("Categories")]
        public int CategoryId { get; set; }

        [JsonIgnore]
        public virtual Category? Category { get; set; }

        [JsonIgnore]
        public virtual List<ProductImages> Images { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public static class ProductExtensions
    {
        public static ProductDTO ToDto(this Product product, string baseUrl)
        {
            if (product == null)
                return null;

            var productDTO = new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = product.Quantity,
                Price = product.Price,
                SmallDescription = product.SmallDescription,
                Description = product.Description,
                CategoryId = product.CategoryId,
                category = new CategoryDto
                {
                    Id = product.Category.Id,
                    Name = product.Category.Name
                }
            };

            var firstImage = product.Images?.FirstOrDefault();
            if (firstImage != null)
            {
                var imagePath = $"/uploads/images/{firstImage.Name}";
                productDTO.Image = $"{baseUrl}{imagePath}";
            }
            return productDTO;
        }

        public static UpdateProductDto ToUpdateDto(this Product product, string baseUrl)
        {
            if (product == null)
                return null;

            var productDTO = new UpdateProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = product.Quantity,
                Price = product.Price,
                SmallDescription = product.SmallDescription,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Images = new()
            };

            var images = product.Images.ToList();
            foreach ( var image in images )
            {
                var imagePath = $"/uploads/images/{image.Name}";
                productDTO.Images.Add(new ProductImages
                {
                    Id = image.Id,
                    Name = $"{baseUrl}{imagePath}",
                });
            }
            return productDTO;
        }
    }
}
