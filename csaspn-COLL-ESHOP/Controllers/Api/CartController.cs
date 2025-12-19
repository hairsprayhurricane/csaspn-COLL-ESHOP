using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using csaspn_COLL_ESHOP.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace csaspn_COLL_ESHOP.Controllers.Api
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(ICartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                await _cartService.AddToCartAsync(request.ProductId, request.Quantity);
                var userId = GetCurrentUserId();
                var count = await _cartService.GetCartItemCountAsync(userId);
                return Ok(new { success = true, count });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            try
            {
                await _cartService.UpdateQuantityAsync(request.CartItemId, request.Quantity);
                var userId = GetCurrentUserId();
                var count = await _cartService.GetCartItemCountAsync(userId);
                
                // Return success response with updated count
                return Ok(new { 
                    success = true, 
                    count,
                    message = "Количество товара обновлено"
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = ex.Message 
                });
            }
        }

        [HttpPost("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                await _cartService.RemoveFromCartAsync(cartItemId);
                var userId = GetCurrentUserId();
                var count = await _cartService.GetCartItemCountAsync(userId);
                return Ok(new { success = true, count });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = GetCurrentUserId();
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Ok(new { count });
        }
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}
