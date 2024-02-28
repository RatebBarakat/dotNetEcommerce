using System.Text.Json.Serialization;

namespace ecommerce.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; } 
        public string Name { get; set; }

        public ICollection<ProductDTO> Products { get; set; } = [];
    }
}
