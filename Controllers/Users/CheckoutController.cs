using ecommerce.Dtos;
using ecommerce.Interfaces;
using ecommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Controllers.Users
{
    [Route("api/user/orders")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public CheckoutController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index() 
        { 
            var orders = await _orderRepository.GetOrders();

            return Ok(orders.Select(o => o.ToDto()));
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CreateOrderDto orderDto)
        {
            await _orderRepository.StartCheckout(orderDto);

            return NoContent();
        }
    }
}
