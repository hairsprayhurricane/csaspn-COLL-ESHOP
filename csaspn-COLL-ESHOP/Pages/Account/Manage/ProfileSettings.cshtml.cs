using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using csaspn_COLL_ESHOP.Models;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;

namespace csaspn_COLL_ESHOP.Areas.Identity.Pages.Account.Manage
{
    public class ProfileSettingsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileSettingsModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string Initials { get; set; }
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Имя")]
            public string FirstName { get; set; }

            [Display(Name = "Фамилия")]
            public string LastName { get; set; }

            [Display(Name = "Номер телефона")]
            [Phone(ErrorMessage = "Некорректный номер телефона")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Аватар")]
            [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Максимальный размер файла 5 МБ")]
            [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".gif" }, ErrorMessage = "Допустимые форматы: .jpg, .jpeg, .png, .gif")]
            public IFormFile? AvatarFile { get; set; }
        }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
        public class MaxFileSizeAttribute : ValidationAttribute
        {
            private readonly int _maxFileSize;
            
            public MaxFileSizeAttribute(int maxFileSize)
            {
                _maxFileSize = maxFileSize;
            }

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value is IFormFile file && file.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage());
                }

                return ValidationResult.Success;
            }

            public string GetErrorMessage()
            {
                return $"Максимальный размер файла {_maxFileSize / (1024 * 1024)}MB.";
            }
        }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
        public class AllowedExtensionsAttribute : ValidationAttribute
        {
            private readonly string[] _extensions;

            public AllowedExtensionsAttribute(string[] extensions)
            {
                _extensions = extensions;
            }

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value is IFormFile file)
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (string.IsNullOrEmpty(extension) || !_extensions.Contains(extension))
                    {
                        return new ValidationResult(GetErrorMessage());
                    }
                }

                return ValidationResult.Success;
            }

            public string GetErrorMessage()
            {
                return $"Разрешены только файлы: {string.Join(", ", _extensions)}";
            }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            AvatarUrl = user.AvatarUrl;
            Initials = !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName)
                ? $"{user.FirstName[0]}{user.LastName[0]}".ToUpper()
                : !string.IsNullOrEmpty(user.UserName) ? user.UserName[0].ToString().ToUpper() : "U";

            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                await LoadAsync(user);
                return Page();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error loading user profile: {ex.Message}");
                // Sign out the user to clear any invalid session
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                if (!ModelState.IsValid)
                {
                    await LoadAsync(user);
                    return Page();
                }

                bool hasChanges = false;

                // Update first name
                if (Input.FirstName != user.FirstName)
                {
                    user.FirstName = Input.FirstName;
                    hasChanges = true;
                }

                // Update last name
                if (Input.LastName != user.LastName)
                {
                    user.LastName = Input.LastName;
                    hasChanges = true;
                }

                // Update phone number
                var currentPhoneNumber = await _userManager.GetPhoneNumberAsync(user);
                if (Input.PhoneNumber != currentPhoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        foreach (var error in setPhoneResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        await LoadAsync(user);
                        return Page();
                    }
                    user.PhoneNumber = Input.PhoneNumber;
                    hasChanges = true;
                }

                // Handle avatar file upload
                if (Input.AvatarFile != null && Input.AvatarFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}_{Input.AvatarFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.AvatarFile.CopyToAsync(fileStream);
                    }

                    user.AvatarUrl = $"/uploads/avatars/{uniqueFileName}";
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        await LoadAsync(user);
                        return Page();
                    }

                    await _signInManager.RefreshSignInAsync(user);
                    TempData["StatusMessage"] = "Профиль успешно обновлен";
                    return RedirectToPage();
                }

                TempData["StatusMessage"] = "Изменения не обнаружены";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in OnPostAsync: {ex}");

                // Set a user-friendly error message
                StatusMessage = "Произошла непредвиденная ошибка при обновлении профиля. Пожалуйста, попробуйте снова.";

                // Try to reload the user data if possible
                try
                {
                    var userId = _userManager.GetUserId(User);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var user = await _userManager.FindByIdAsync(userId);
                        if (user != null)
                        {
                            await LoadAsync(user);
                        }
                    }
                }
                catch
                {
                    // If we can't reload the user, just continue
                }

                return Page();
            }
        }
    }
}
