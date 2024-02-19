using System.Linq;
using ecommerce.Models;

namespace ecommerce.Dtos
{
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string SmallDescription { get; set; }
        public string Description { get; set; }
        public List<ProductImages> Images { get; set; }
        public int CategoryId { get; set; }
    }
}
