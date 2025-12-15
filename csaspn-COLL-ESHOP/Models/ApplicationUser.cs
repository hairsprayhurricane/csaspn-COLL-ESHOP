using Microsoft.AspNetCore.Identity;

namespace csaspn_COLL_ESHOP.Models;

public class ApplicationUser : IdentityUser
{
    // Add custom user properties here (if needed in the future)
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
