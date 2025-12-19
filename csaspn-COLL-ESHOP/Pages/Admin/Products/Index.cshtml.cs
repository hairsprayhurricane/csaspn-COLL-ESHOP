using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using csaspn_COLL_ESHOP.Models;
using csaspn_COLL_ESHOP.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace csaspn_COLL_ESHOP.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IList<Product> Products { get; set; } = new List<Product>();
        public IList<Category> Categories { get; set; } = new List<Category>();

        [BindProperty]
        public CategoryInputModel CategoryInput { get; set; } = new CategoryInputModel();

        public class CategoryInputModel
        {
            [Required(ErrorMessage = "Название категории обязательно")]
            [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
            public string Name { get; set; }

            [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
            public string Description { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAddCategoryAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    ModelState.AddModelError("name", "Название категории обязательно");
                }
                else
                {
                    var category = new Category
                    {
                        Name = name.Trim()
                    };

                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Категория \"{name}\" успешно добавлена";
                    return RedirectToPage();
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении категории");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при сохранении категории. Возможно, категория с таким именем уже существует.");
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                try
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Категория \"{category.Name}\" удалена";
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении категории");
                    TempData["ErrorMessage"] = "Не удалось удалить категорию. Убедитесь, что нет товаров, связанных с этой категорией.";
                }
            }

            return RedirectToPage();
        }
    }
}
