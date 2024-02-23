using ecommerce.Dtos;
using ecommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Interfaces
{
    public interface ICartRepository
    {
        public Task<bool> AddToCart(ProductCartDto productCartDto);
        public Task<bool> RemoveFromCart(int id);
        public Task<bool> UpdateInCart(int id, UpdateProductCartDto productCartDto);
        public Task<IEnumerable<CartDto>?> GetItems();
    }
}
