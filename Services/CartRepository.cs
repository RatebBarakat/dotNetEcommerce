using ecommerce.Data;
using ecommerce.Dtos;
using ecommerce.Interfaces;
using ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ecommerce.Hepers
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        public CartRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        private async Task<User?> GetUserAsync()
        {
            var email = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users
                .Include(u => u.Carts)
                .ThenInclude(c => c.Product)
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
            if (user is null)
            {
                throw new Exception("user is not authenticated");
            }
            return user;
        }

        public async Task<bool> AddToCart(ProductCartDto productCartDto)
        {
            var user = await GetUserAsync();
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productCartDto.ProductId);
            if (product is null)
            {
                throw new Exception("product not found");
            }

            return await UpsertCart(user, productCartDto, product);
        }

        private async Task<bool> UpsertCart(User user, ProductCartDto productCartDto, Product product)
        {
            var cart = user.Carts.Where(c => c.ProductId == product.Id).FirstOrDefault();
            if (cart is null)
            {
                user.Carts.Add(new Cart
                {
                    ProductId = product.Id,
                    Quantity = productCartDto.Quantity,
                    UserId = user.Id
                });
            }
            else
            {
                cart.Quantity += productCartDto.Quantity;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromCart(int id)
        {
            var user = await GetUserAsync();
            var cart = await _context.Carts.Where(c => c.UserId == user.Id).FirstOrDefaultAsync(c => c.Id == id);

            if (cart is null)
            {
                throw new Exception("item not found");
            }

            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateInCart(int id, UpdateProductCartDto productCartDto)
        {
            var user = await GetUserAsync();
            var cart = user.Carts.Where(c => c.Id == id).FirstOrDefault();

            if (cart is null)
            {
                throw new Exception("item not found");
            }

            cart.Quantity += productCartDto.Quantity;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CartDto>?> GetItems()
        {
            var user = await GetUserAsync();
            var carts = user.Carts.Select(c => new CartDto
            {
                Id = c.Id,
                Quantity = c.Quantity,
                ProductId = c.ProductId,
                Product = c.Product
            });
            return carts;
        }
    }
}
