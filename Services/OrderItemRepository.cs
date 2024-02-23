using ecommerce.Interfaces;
using ecommerce.Models;

namespace ecommerce.Services
{
    public class OrderItemRepository : IOrderItemRepository
    {
        public bool Add(Order order, OrderItem orderItem)
        {
            order.Items.Add(orderItem);
            return true;
        }
    }
}
