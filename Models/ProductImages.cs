using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class ProductImages
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [ForeignKey("Products")]
        public int ProductId { get; set; }
        [JsonIgnore]
        public virtual Product product { get; set; }
    }
}
