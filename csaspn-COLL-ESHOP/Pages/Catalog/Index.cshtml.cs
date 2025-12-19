using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using csaspn_COLL_ESHOP.Models;
using Microsoft.AspNetCore.Authorization;
using csaspn_COLL_ESHOP.Data;
using csaspn_COLL_ESHOP.Services;

namespace csaspn_COLL_ESHOP.Pages.Catalog
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly ICartService _cartService;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger, ICartService cartService)
        {
            _context = context;
            _logger = logger;
            _cartService = cartService;
        }

        public IList<Product> Products { get; set; } = new List<Product>();
        public IList<Category> Categories { get; set; } = new List<Category>();
        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            // Get all categories for the filter
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            // Base query
            var products = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Apply category filter
            if (CategoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == CategoryId.Value);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchString))
            {
                var searchTerm = SearchString.ToLower();
                products = products.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) || 
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
            }

            // Execute the query and get results
            Products = await products.OrderByDescending(p => p.CreatedDate).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId)
        {
            try
            {
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

                await _cartService.AddToCartAsync(productId, 1);
                TempData["SuccessMessage"] = $"Товар {product.Name} добавлен в корзину";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении товара в корзину");
                TempData["ErrorMessage"] = "Произошла ошибка при добавлении товара в корзину";
            }

            // Сохраняем параметры фильтрации в TempData для использования после редиректа
            if (CategoryId.HasValue)
            {
                TempData["CategoryId"] = CategoryId.Value;
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                TempData["SearchString"] = SearchString;
            }

            return RedirectToPage();
        }
    }
}
