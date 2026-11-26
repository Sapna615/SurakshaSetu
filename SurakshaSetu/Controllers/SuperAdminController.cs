using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using SurakshaSetu.Models;
using SurakshaSetu.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SurakshaSetu.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SuperAdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Dashboard()
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "SuperAdmin";

            var roles = await _userManager.GetRolesAsync(user);

            var model = new SecurityProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Department = user.Department ?? "System Administration",
                Roles = roles.ToList(),
                CreatedAt = System.DateTime.UtcNow.AddMonths(-6)
            };

            return View("~/Views/Security/Profile.cshtml", model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "SuperAdmin";
            return View("~/Views/Security/ChangePassword.cshtml", new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "SuperAdmin";

            if (!ModelState.IsValid) return View("~/Views/Security/ChangePassword.cshtml", model);

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("~/Views/Security/ChangePassword.cshtml", model);
            }

            TempData["SuccessMessage"] = "Your password has been changed.";
            return RedirectToAction(nameof(Profile));
        }
    }
}
