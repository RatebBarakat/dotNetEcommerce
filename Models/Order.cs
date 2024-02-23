using ecommerce.Dtos;
using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string Address { get; set; }


        public decimal Total { get; set; }
        public string UserId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; }

        [JsonIgnore]
        public virtual List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public static class OrderExtentions
    {
        public static OrderDto ToDto(this Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                Address = order.Address,
                Total = order.Total,
                Status = order.Status,
                Items = order.Items,
            };
        }
    }
}
