using ecommerce.Dtos;
using ecommerce.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Controllers.Users
{
    [Route("user/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetCarts()
        {
            var carts = await _cartRepository.GetItems();
            return Ok(carts);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(ProductCartDto productCartDto)
        {
            await _cartRepository.AddToCart(productCartDto);

            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCart(UpdateProductCartDto productCartDto)
        {
            await _cartRepository.UpdateInCart(productCartDto);

            return NoContent();
        }

        [HttpDelete("{id}")]

        public async Task<IActionResult> AddToCart(int id)
        {
            await _cartRepository.RemoveFromCart(id);

            return NoContent();
        }
    }
}
