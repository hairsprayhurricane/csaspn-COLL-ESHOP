using Microsoft.AspNetCore.Identity;

namespace csaspn_COLL_ESHOP.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
}