using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using SurakshaSetu.Models;
using SurakshaSetu.ViewModels;

namespace SurakshaSetu.Controllers
{
    // SECURITY: This absolutely guarantees that ONLY logged-in users with the 'Student' role can access this controller.
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            // 1. Securely identify the exact student making the request
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            // 2. Query Entity Framework Core for ONLY this student's reports.
            var myReports = await _context.SafetyReports
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // 3. Calculate REAL statistics dynamically from the database
            int pending = myReports.Count(r => r.Status == "Pending");
            int review = myReports.Count(r => r.Status == "Under Review");
            int investigation = myReports.Count(r => r.Status == "Investigation");
            int resolved = myReports.Count(r => r.Status == "Resolved" || r.Status == "Action Taken");
            
            // Get Active Alerts count
            int activeAlertsCount = await _context.SafetyAlerts
                .CountAsync(s => s.IsActive && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow));

            // Fetch top 3 latest active alerts
            var activeAlerts = await _context.SafetyAlerts
                .Where(s => s.IsActive && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(s => s.PublishedAt)
                .Take(3)
                .ToListAsync();

            var emergencyContacts = await _context.EmergencyContacts
                .Where(c => c.IsActive)
                .Take(4)
                .ToListAsync();

            // 4. Pack the secure data into our ViewModel
            var viewModel = new StudentDashboardViewModel
            {
                StudentName = user.FullName, // The user requested "My Name" instead of Gmail
                TotalReports = myReports.Count,
                PendingReports = pending + review,
                InvestigationReports = investigation,
                ResolvedReports = resolved,
                RecentReports = myReports.Take(3).ToList(), // Send only the latest 3 for the preview table
                UnreadNotificationsCount = 0,
                ActiveAlertsCount = activeAlertsCount,
                ActiveAlerts = activeAlerts,
                EmergencyContacts = emergencyContacts
            };

            ViewData["UserName"] = user.FullName;

            // 5. Send the ViewModel to the View
            return View(viewModel);
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Student";

            var roles = await _userManager.GetRolesAsync(user);

            var model = new SecurityProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Department = user.Department ?? "Student",
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
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Student";
            return View("~/Views/Security/ChangePassword.cshtml", new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Student";

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
