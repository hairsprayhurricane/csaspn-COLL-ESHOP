using csaspn_COLL_ESHOP.Models;

namespace csaspn_COLL_ESHOP.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public string? SessionId { get; set; }
        public string? UserId { get; set; }
    }
}
