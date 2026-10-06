using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using SurakshaSetu.Models;
using System.Linq;
using System.Threading.Tasks;
using SurakshaSetu.ViewModels;

namespace SurakshaSetu.Controllers
{
    [Authorize(Policy = "ReadOnlyAnalytics")]
    public class ManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Management";
            ViewData["Title"] = "Executive Summary";

            var reports = await _context.SafetyReports.AsNoTracking().ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                AdminName = user.FullName ?? "Management",
                TotalReports = reports.Count,
                OpenCases = reports.Count(r => r.Status != "Resolved" && r.Status != "Cancelled"),
                ActiveSOS = reports.Count(r => r.ImmediateDanger && r.Status != "Resolved" && r.Status != "Cancelled"),
                ResolvedCases = reports.Count(r => r.Status == "Resolved"),
                
                RecentReports = reports.OrderByDescending(r => r.CreatedAt).Take(10).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Reports()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Management";
            ViewData["Title"] = "Analytics Reports";

            var incidents = await _context.SafetyReports
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var viewModel = new SecurityIncidentQueueViewModel
            {
                Incidents = incidents
            };

            return View(viewModel);
        }
    }
}
