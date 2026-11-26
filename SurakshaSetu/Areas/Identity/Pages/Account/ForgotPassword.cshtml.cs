using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SurakshaSetu.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SurakshaSetu.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress(ErrorMessage = "Please enter a valid email address (e.g., email@gmail.com).")]
            public string Email { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);

            // Always show redirect to avoid email enumeration,
            // but only generate a real token if the user exists.
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // For dev: if email not confirmed, confirm it automatically
                if (user != null)
                {
                    var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, confirmToken);
                }
                else
                {
                    // User doesn't exist — redirect to reset page with a dummy notice
                    TempData["InfoMessage"] = "If that email is registered, a reset link has been generated below.";
                    return RedirectToPage("./ResetPassword");
                }
            }

            // Generate the reset token and redirect directly to the ResetPassword page
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            return RedirectToPage("./ResetPassword", new { code, email = Input.Email });
        }
    }
}

