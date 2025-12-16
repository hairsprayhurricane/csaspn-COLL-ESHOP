using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using csaspn_COLL_ESHOP.Models;
using System.Threading.Tasks;

namespace csaspn_COLL_ESHOP.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AvatarUrl { get; set; }
        public string Initials { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Username = user.UserName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            FirstName = user.FirstName;
            LastName = user.LastName;
            
            // Set avatar URL (either Base64 string or null)
            AvatarUrl = user.AvatarUrl;
            
            // Generate initials for avatar if no avatar is set
            if (string.IsNullOrEmpty(AvatarUrl))
            {
                if (!string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName))
                {
                    Initials = $"{user.FirstName[0]}{user.LastName[0]}".ToUpper();
                }
                else if (!string.IsNullOrEmpty(user.UserName))
                {
                    Initials = user.UserName[0].ToString().ToUpper();
                }
                else
                {
                    Initials = "U";
                }
            }

            return Page();
        }
    }
}
