
using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [JsonIgnore]
        public virtual Product Product { get; set; }

        public int Quantity { get; set; }
        public string UserId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
    }
}
