using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal Quantity { get; set; }

        public int OrderId { get; set; }
        [JsonIgnore]
        public virtual Order Order { get; set; }
    }
}
