using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using csaspn_COLL_ESHOP.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace csaspn_COLL_ESHOP.Pages.Cart
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IndexModel(ICartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        public CartViewModel Cart { get; set; } = new CartViewModel();

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            Cart = await _cartService.GetCartViewModelAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int cartItemId, int quantity)
        {
            await _cartService.UpdateQuantityAsync(cartItemId, quantity);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveFromCartAsync(int cartItemId)
        {
            await _cartService.RemoveFromCartAsync(cartItemId);
            return RedirectToPage();
        }
    }
}