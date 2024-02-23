using ecommerce.Models;

namespace ecommerce.Dtos
{
    public class CartDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public Product Product { get; set; }
    }
}
