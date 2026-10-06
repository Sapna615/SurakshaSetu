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
    [Authorize(Policy = "ManageHostel")]
    public class WardenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WardenController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Warden";
            ViewData["Title"] = "Hostel Overview";

            var reports = await _context.SafetyReports
                .Where(r => r.IncidentLocation.Contains("Hostel"))
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new SecurityDashboardViewModel
            {
                OfficerName = user.FullName ?? "Warden",
                NewReports = reports.Count(r => r.Status == "Pending" || r.Status == "Submitted"),
                InProgress = reports.Count(r => r.Status == "In Progress" || r.Status == "Under Investigation"),
                
                IncidentQueue = reports.Where(r => r.Status != "Resolved" && r.Status != "Cancelled")
                                       .OrderByDescending(r => r.CreatedAt)
                                       .Take(10).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Incidents()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Warden";
            ViewData["Title"] = "Hostel Incidents";

            var incidents = await _context.SafetyReports
                .AsNoTracking()
                .Where(r => r.IncidentLocation.Contains("Hostel"))
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
