using ecommerce.Dtos;
using ecommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Interfaces
{
    public interface IOrderItemRepository
    {
        public bool Add(Order order, OrderItem orderItem);
    }
}
