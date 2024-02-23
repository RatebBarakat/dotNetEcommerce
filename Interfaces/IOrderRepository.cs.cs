using ecommerce.Dtos;
using ecommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Interfaces
{
    public interface IOrderRepository
    {
        public Task<bool> StartCheckout(CreateOrderDto createOrderDto);
        public Task<IEnumerable<Order>?> GetOrders();
    }
}
