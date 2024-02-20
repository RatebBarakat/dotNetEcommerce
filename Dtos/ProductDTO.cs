using System.Linq;
using System.Text.Json.Serialization;
using ecommerce.Models;

namespace ecommerce.Dtos
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string SmallDescription { get; set; }
        public string Description { get; set; }
        public string image { get; set; }
        public int CategoryId { get; set; }
        public CategoryDto? category { get; set; }
    }
}
