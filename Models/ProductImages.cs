using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.Models
{
    public class ProductImages
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [ForeignKey("Products")]
        public int ProductId { get; set; }

        public Product product { get; set; }
    }
}
