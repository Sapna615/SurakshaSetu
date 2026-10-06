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
    [Authorize(Policy = "ManageCounseling")]
    public class CounselorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CounselorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Counselor";
            ViewData["Title"] = "Dashboard";

            var reports = await _context.SafetyReports
                .Where(r => r.IncidentCategory == "Harassment" || r.IncidentCategory == "Sexual Harassment" || r.IncidentCategory == "Bullying")
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new SecurityDashboardViewModel
            {
                OfficerName = user.FullName ?? "Counselor",
                NewReports = reports.Count(r => r.Status == "Pending" || r.Status == "Submitted"),
                InProgress = reports.Count(r => r.Status == "In Progress" || r.Status == "Under Investigation"),
                
                IncidentQueue = reports.Where(r => r.Status != "Resolved" && r.Status != "Cancelled")
                                       .OrderByDescending(r => r.CreatedAt)
                                       .Take(10).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Cases()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Counselor";
            ViewData["Title"] = "Sensitive Cases";

            var incidents = await _context.SafetyReports
                .AsNoTracking()
                .Where(r => r.IncidentCategory == "Harassment" || r.IncidentCategory == "Sexual Harassment" || r.IncidentCategory == "Bullying")
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
