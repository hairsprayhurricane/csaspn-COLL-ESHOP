using Microsoft.AspNetCore.Mvc;
using csaspn_COLL_ESHOP.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace csaspn_COLL_ESHOP.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartSummaryViewComponent(ICartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = GetCurrentUserId();
            var cartCount = await _cartService.GetCartItemCountAsync(userId);
            return View(cartCount);
        }
    }
}