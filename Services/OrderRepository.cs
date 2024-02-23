using ecommerce.Data;
using ecommerce.Dtos;
using ecommerce.Interfaces;
using ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System;
using System.Security.Claims;

namespace ecommerce.Services
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderRepository(IOrderItemRepository orderItemRepository, AppDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _orderItemRepository = orderItemRepository;
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


        public async Task<IEnumerable<Order>?> GetOrders()
        {
            var user = await GetUserAsync();

            await _context.Entry(user)
                .Collection(u => u.Orders)
                .Query()
                .Include(o => o.Items)
                .LoadAsync();

            return user.Orders;
        }

        private decimal CalculateTotal(ICollection<Cart> carts)
        {
            decimal total = 0;

            foreach (var cart in carts)
            {
                total += cart.Quantity * cart.Product.Price;
            }

            return total;
        }

        public async Task<bool> StartCheckout(CreateOrderDto createOrderDto)
        {
            var user = await GetUserAsync();

            var total = CalculateTotal(user.Carts);

            var order = new Order
            {
                Address = createOrderDto.Address,
                Status = "processing",
                Total = total,
            };

            if (user.Carts.Any())
            {
                var orderItems = user.Carts.Select(cartItem => new OrderItem
                {
                    Name = cartItem.Product.Name,
                    Price = cartItem.Product.Price,
                    Quantity = cartItem.Quantity,

                }).ToList();

                foreach (var item in orderItems)
                {
                    _orderItemRepository.Add(order, item);
                }

                user.Orders.Add(order);

                user.Carts.Clear();

                await _context.SaveChangesAsync();

                return true;
            }
            else
            {
                throw new Exception("cart is empty!");
            }
        }
    }
}