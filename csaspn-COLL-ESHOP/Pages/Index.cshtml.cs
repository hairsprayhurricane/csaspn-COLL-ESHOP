using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using csaspn_COLL_ESHOP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using csaspn_COLL_ESHOP.Data;
using csaspn_COLL_ESHOP.Services;
using Microsoft.AspNetCore.Http;

namespace csaspn_COLL_ESHOP.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ILogger<IndexModel> logger,
            ApplicationDbContext context,
            ICartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _cartService = cartService;
            _userManager = userManager;
        }

        public IList<Product> Products { get; set; } = new List<Product>();
        public IList<CartItem> CartItems { get; set; } = new List<CartItem>();
        public bool IsAuthenticated { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
            
            if (IsAuthenticated)
            {
                // Load products
                Products = await _context.Products
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(12)
                    .ToListAsync();

                // Load user's cart items
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    CartItems = await _context.CartItems
                        .Where(ci => ci.UserId == user.Id)
                        .ToListAsync();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Пожалуйста, войдите в систему, чтобы добавить товар в корзину";
                return RedirectToPage("/Account/Login", new { returnUrl = "/" });
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToPage();

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Товар не найден";
                    return RedirectToPage();
                }

                if (product.StockQuantity <= 0)
                {
                    TempData["ErrorMessage"] = "Товар отсутствует на складе";
                    return RedirectToPage();
                }

                // Check if item already in cart
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == user.Id && ci.ProductId == productId);

                if (cartItem == null)
                {
                    // Add new item to cart
                    cartItem = new CartItem
                    {
                        UserId = user.Id,
                        ProductId = productId,
                        Quantity = 1,
                        AddedAt = DateTime.UtcNow
                    };
                    _context.CartItems.Add(cartItem);
                    TempData["SuccessMessage"] = $"Товар {product.Name} добавлен в корзину";
                }
                else
                {
                    // Update quantity if already in cart
                    if (cartItem.Quantity >= product.StockQuantity)
                    {
                        TempData["ErrorMessage"] = $"Недостаточно товара на складе. Доступно: {product.StockQuantity} шт.";
                        return RedirectToPage();
                    }

                    cartItem.Quantity++;
                    cartItem.UpdatedAt = DateTime.UtcNow;
                    TempData["SuccessMessage"] = $"Количество товара {product.Name} обновлено: {cartItem.Quantity} шт.";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении товара в корзину");
                TempData["ErrorMessage"] = "Произошла ошибка при добавлении товара в корзину";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateCartAsync(int productId, int change)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Пожалуйста, войдите в систему, чтобы изменить количество товара";
                return RedirectToPage("/Account/Login", new { returnUrl = "/" });
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToPage();

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Товар не найден";
                    return RedirectToPage();
                }

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == user.Id && ci.ProductId == productId);

                if (cartItem == null)
                {
                    if (change > 0)
                    {
                        // Add new item if increasing quantity
                        cartItem = new CartItem
                        {
                            UserId = user.Id,
                            ProductId = productId,
                            Quantity = 1,
                            AddedAt = DateTime.UtcNow
                        };
                        _context.CartItems.Add(cartItem);
                        TempData["SuccessMessage"] = $"Товар {product.Name} добавлен в корзину";
                    }
                }
                else
                {
                    var newQuantity = cartItem.Quantity + change;

                    if (newQuantity <= 0)
                    {
                        // Remove item if quantity becomes zero or negative
                        _context.CartItems.Remove(cartItem);
                        TempData["SuccessMessage"] = $"Товар {product.Name} удален из корзины";
                    }
                    else if (newQuantity > product.StockQuantity)
                    {
                        TempData["ErrorMessage"] = $"Недостаточно товара на складе. Доступно: {product.StockQuantity} шт.";
                        return RedirectToPage();
                    }
                    else
                    {
                        // Update quantity
                        cartItem.Quantity = newQuantity;
                        cartItem.UpdatedAt = DateTime.UtcNow;
                        TempData["SuccessMessage"] = $"Количество товара {product.Name} обновлено: {newQuantity} шт.";
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении корзины");
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении корзины";
            }

            return RedirectToPage();
        }
    }
}