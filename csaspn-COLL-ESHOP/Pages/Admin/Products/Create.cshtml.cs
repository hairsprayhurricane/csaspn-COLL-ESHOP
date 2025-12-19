using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using csaspn_COLL_ESHOP.Data;
using csaspn_COLL_ESHOP.Models;
using Microsoft.EntityFrameworkCore;

namespace csaspn_COLL_ESHOP.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            return Page();

            // Set default image if not provided
            if (string.IsNullOrEmpty(Product.ImageUrl))
            {
                Product.ImageUrl = "/images/placeholder.jpg";
            }

            Product.CreatedDate = DateTime.Now;
            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
