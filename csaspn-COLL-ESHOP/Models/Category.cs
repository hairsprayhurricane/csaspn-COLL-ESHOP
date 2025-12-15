using System.ComponentModel.DataAnnotations;

namespace csaspn_COLL_ESHOP.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required (ErrorMessage = "Название категории обязательно")]
        [Display(Name = "Название категории")]
        [StringLength (50, ErrorMessage = "Максимум 50 символов")]
        public string Name { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new();
    }
}
