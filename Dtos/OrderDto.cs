using ecommerce.Models;

namespace ecommerce.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }

        public string Address { get; set; }


        public decimal Total { get; set; }

        public string Status { get; set; }

        public virtual List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

}
