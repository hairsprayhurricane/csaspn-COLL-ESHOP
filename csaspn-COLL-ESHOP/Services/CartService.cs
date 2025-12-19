using csaspn_COLL_ESHOP.Data;
using csaspn_COLL_ESHOP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace csaspn_COLL_ESHOP.Services
{
    public interface ICartService
    {
        Task AddToCartAsync(int productId, int quantity = 1);
        Task RemoveFromCartAsync(int cartItemId);
        Task UpdateQuantityAsync(int cartItemId, int quantity);
        Task<int> GetCartItemCountAsync(string userId);
        Task<CartViewModel> GetCartViewModelAsync(string userId);
    }

    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddToCartAsync(int productId, int quantity = 1)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }

            var cartItem = await GetOrCreateCartItemAsync(userId, productId);
            cartItem.Quantity += quantity;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await GetCartItemByIdAsync(userId, cartItemId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateQuantityAsync(int cartItemId, int quantity)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await GetCartItemByIdAsync(userId, cartItemId);
            
            if (cartItem == null)
                throw new ArgumentException("Cart item not found");

            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product == null)
                throw new ArgumentException("Product not found");

            // If quantity is 0 or negative, remove the item
            if (quantity <= 0)
            {
                await RemoveFromCartAsync(cartItemId);
                return;
            }

            // Check if requested quantity is available in stock
            if (quantity > product.StockQuantity)
            {
                throw new InvalidOperationException($"В наличии только {product.StockQuantity} шт. этого товара");
            }

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }

        public async Task<CartViewModel> GetCartViewModelAsync(string userId)
    {
        var items = await _context.CartItems
            .Include(ci => ci.Product)
            .ThenInclude(p => p.Category)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        return new CartViewModel
        {
            Items = items.Select(ci => new CartItemViewModel
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product?.Name ?? "Unknown Product",
                Price = ci.Product?.Price ?? 0,
                ImageUrl = ci.Product?.ImageUrl ?? "/images/placeholder.jpg",
                Quantity = ci.Quantity,
                StockQuantity = ci.Product?.StockQuantity ?? 0,
                Product = ci.Product != null ? new ProductViewModel
                {
                    Id = ci.Product.Id,
                    Name = ci.Product.Name,
                    Price = ci.Product.Price,
                    ImageUrl = ci.Product.ImageUrl,
                    StockQuantity = ci.Product.StockQuantity
                } : null
            }).ToList()
        };
    }

        private async Task<CartItem> GetOrCreateCartItemAsync(string userId, int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new ArgumentException("Product not found", nameof(productId));
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    UserId = userId,
                    Quantity = 0
                };
                _context.CartItems.Add(cartItem);
            }

            return cartItem;
        }

        private async Task<CartItem> GetCartItemByIdAsync(string userId, int cartItemId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);
        }
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal Total => Items.Sum(i => i.Total);
    }

    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public int StockQuantity { get; set; }
        public ProductViewModel? Product { get; set; }
    }

    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
    }
}