using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using SurakshaSetu.Models;
using SurakshaSetu.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SurakshaSetu.Controllers
{
    [Authorize(Roles = "SecurityOfficer,Admin")]
    public class SecurityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SecurityController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "SecurityOfficer")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reports = await _context.SafetyReports.AsNoTracking().ToListAsync();
            
            var viewModel = new SecurityDashboardViewModel
            {
                OfficerName = user.FullName ?? user.UserName ?? "Officer",
                NewReports = reports.Count(r => r.Status == "Pending" || r.Status == "Submitted"),
                ActiveSOS = reports.Count(r => r.ImmediateDanger && r.Status != "Resolved" && r.Status != "Cancelled"),
                InProgress = reports.Count(r => r.Status == "In Progress" || r.Status == "Under Investigation"),
                
                ActiveSOSList = reports.Where(r => r.ImmediateDanger && r.Status != "Resolved" && r.Status != "Cancelled")
                                     .OrderByDescending(r => r.CreatedAt)
                                     .Take(5).ToList(),
                
                IncidentQueue = reports.Where(r => r.Status != "Resolved" && r.Status != "Cancelled")
                                       .OrderByDescending(r => r.CreatedAt)
                                       .Take(10).ToList()
            };

            ViewData["UserName"] = viewModel.OfficerName;
            ViewData["RoleName"] = "Security Officer";
            
            return View(viewModel);
        }
        [Authorize(Roles = "SecurityOfficer")]
        public async Task<IActionResult> IncidentQueue(string? search, string? category, string? severity, string? status, string? sortBy)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Officer";

            var query = _context.SafetyReports.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(r => r.ReportId.Contains(search) || r.Description.Contains(search) || r.IncidentLocation.Contains(search));
            
            if (!string.IsNullOrEmpty(category) && category != "All Categories")
                query = query.Where(r => r.IncidentCategory == category);
                
            if (!string.IsNullOrEmpty(severity) && severity != "All Severities")
                query = query.Where(r => r.Severity == severity);
                
            if (!string.IsNullOrEmpty(status) && status != "All Statuses")
                query = query.Where(r => r.Status == status);

            if (sortBy == "Oldest First")
                query = query.OrderBy(r => r.CreatedAt);
            else
                query = query.OrderByDescending(r => r.CreatedAt);

            var incidents = await query.ToListAsync();
            var categories = new List<string> { 
                "Harassment", 
                "Sexual Harassment", 
                "Theft", 
                "Bullying", 
                "Stalking", 
                "Unsafe Area", 
                "Infrastructure Issue", 
                "Medical Emergency", 
                "Other" 
            };

            // Add any other categories that are actually in the DB but not in the static list
            var dbCategories = incidents.Select(i => i.IncidentCategory).Distinct();
            foreach(var cat in dbCategories) {
                if(!categories.Contains(cat)) {
                    categories.Add(cat);
                }
            }

            var viewModel = new SecurityIncidentQueueViewModel
            {
                Incidents = incidents,
                SearchQuery = search,
                Category = category,
                Severity = severity,
                Status = status,
                SortBy = sortBy,
                AvailableCategories = categories
            };

            return View("IncidentQueue", viewModel);
        }
        [Authorize(Roles = "SecurityOfficer")]
        public async Task<IActionResult> MyCases()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Officer";

            var incidents = await _context.SafetyReports
                .AsNoTracking()
                .Where(r => r.AssignedTo == user.Email && r.Status != "Resolved" && r.Status != "Cancelled")
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .ToListAsync();

            var viewModel = new SecurityIncidentQueueViewModel
            {
                Incidents = incidents
            };

            return View(viewModel);
        }

        [Authorize(Roles = "SecurityOfficer")]
        public async Task<IActionResult> Infrastructure(string? search, string? severity, string? status, string? sortBy)
        {
            ViewData["Title"] = "Infrastructure Issues";
            return await IncidentQueue(search, "Infrastructure Issue", severity, status, sortBy);
        }

        [Authorize(Roles = "SecurityOfficer")]
        public async Task<IActionResult> EmergencyRequests(string? search, string? severity, string? status, string? sortBy)
        {
            ViewData["Title"] = "Emergency Requests";
            return await IncidentQueue(search, "Emergency Request", severity, status, sortBy);
        }

        [Authorize(Roles = "SecurityOfficer")]
        public async Task<IActionResult> ActiveSOS()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Officer";

            var incidents = await _context.SafetyReports
                .AsNoTracking()
                .Where(r => r.ImmediateDanger && r.Status != "Resolved" && r.Status != "Cancelled")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var viewModel = new SecurityIncidentQueueViewModel
            {
                Incidents = incidents
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Officer";

            var roles = await _userManager.GetRolesAsync(user);

            var model = new SecurityProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Department = user.Department ?? "Security",
                Roles = roles.ToList(),
                CreatedAt = System.DateTime.UtcNow.AddMonths(-3) // Mock date since it's not on ApplicationUser
            };

            return View(model);
        }
        
        public async Task<IActionResult> IncidentDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Officer";

            var incident = await _context.SafetyReports
                .Include(r => r.StatusHistory)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.SafetyReportId == id);

            if (incident == null) return NotFound();

            ApplicationUser? reporter = null;
            if (!incident.IsAnonymous && !string.IsNullOrEmpty(incident.UserId))
            {
                reporter = await _userManager.FindByIdAsync(incident.UserId);
            }

            ApplicationUser? assignedOfficer = null;
            if (!string.IsNullOrEmpty(incident.AssignedTo))
            {
                assignedOfficer = await _userManager.FindByEmailAsync(incident.AssignedTo);
            }
            
            var location = await _context.CampusLocations.AsNoTracking()
                .FirstOrDefaultAsync(l => l.CampusLocationId.ToString() == incident.IncidentLocation || l.Name == incident.IncidentLocation);

            var securityOfficers = await _userManager.GetUsersInRoleAsync("SecurityOfficer");

            var viewModel = new SecurityIncidentDetailsViewModel
            {
                Incident = incident,
                Reporter = reporter,
                AssignedOfficer = assignedOfficer,
                LocationName = location?.Name ?? incident.IncidentLocation,
                AvailableOfficers = securityOfficers.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateIncidentStatus(int id, string newStatus, string responseNote, string assignToEmail)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var incident = await _context.SafetyReports.FirstOrDefaultAsync(r => r.SafetyReportId == id);
            if (incident == null) return NotFound();

            if (!string.IsNullOrEmpty(newStatus))
            {
                incident.Status = newStatus;
                if (newStatus == "Resolved")
                {
                    incident.ResolvedAt = System.DateTime.UtcNow;
                }
            }
            
            if (!string.IsNullOrEmpty(responseNote))
            {
                incident.ResolutionNotes = string.IsNullOrEmpty(incident.ResolutionNotes) 
                    ? $"[{System.DateTime.UtcNow:yyyy-MM-dd HH:mm} - {user.FullName}] {responseNote}"
                    : incident.ResolutionNotes + $"\n[{System.DateTime.UtcNow:yyyy-MM-dd HH:mm} - {user.FullName}] {responseNote}";
            }
            
            if (!string.IsNullOrEmpty(assignToEmail))
            {
                incident.AssignedTo = assignToEmail;
                if (incident.Status == "Pending" || incident.Status == "Submitted")
                {
                    incident.Status = "Assigned";
                }
            }

            incident.UpdatedAt = System.DateTime.UtcNow;
            
            var history = new SafetyReportStatusHistory
            {
                SafetyReportId = incident.SafetyReportId,
                NewStatus = incident.Status,
                ChangedBy = user.Id,
                ChangedAt = System.DateTime.UtcNow,
                Comment = responseNote
            };
            _context.Set<SafetyReportStatusHistory>().Add(history);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Incident updated successfully.";
            return RedirectToAction(nameof(IncidentDetails), new { id = incident.SafetyReportId });
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Officer";
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Officer";

            if (!ModelState.IsValid) return View(model);

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _userManager.UpdateSecurityStampAsync(user);
            TempData["StatusMessage"] = "Your password has been changed successfully.";
            return RedirectToAction(nameof(Profile));
        }
    }
}
