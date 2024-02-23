using ecommerce.Dtos;
using ecommerce.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Controllers.Users
{
    [ApiController]
    [Route("api/user/cart")]
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
            try
            {
                var carts = await _cartRepository.GetItems();
                return Ok(carts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(ProductCartDto productCartDto)
        {
            try
            {
                await _cartRepository.AddToCart(productCartDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCart(int id, UpdateProductCartDto productCartDto)
        {
            try
            {
                await _cartRepository.UpdateInCart(id, productCartDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]

        public async Task<IActionResult> AddToCart(int id)
        {
            try
            {
                await _cartRepository.RemoveFromCart(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NoContent();
        }
    }
}
