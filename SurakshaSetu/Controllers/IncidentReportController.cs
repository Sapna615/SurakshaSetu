using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using SurakshaSetu.Models;
using SurakshaSetu.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SurakshaSetu.Controllers
{
    [Authorize]
    public class IncidentReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IncidentReportController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /IncidentReport/Create
        [HttpGet]
        public IActionResult Create(string source = "Dashboard")
        {
            var model = new SafetyReportCreateViewModel
            {
                Source = source
            };
            return View(model);
        }

        // POST: /IncidentReport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SafetyReportCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (model.IncidentDate.Date > DateTime.Today)
            {
                ModelState.AddModelError("IncidentDate", "Incident date cannot be in the future.");
                return View(model);
            }

            // Generate unique Report ID
            string reportId;
            do
            {
                reportId = "SS-" + DateTime.Now.ToString("yyMMdd") + "-" + Random.Shared.Next(1000, 9999);
            }
            while (await _context.SafetyReports.AnyAsync(r => r.ReportId == reportId));

            string finalCategory = model.IncidentCategory;
            if (model.IncidentCategory == "Other" && !string.IsNullOrWhiteSpace(Request.Form["OtherCategory"]))
            {
                finalCategory = "Other: " + Request.Form["OtherCategory"].ToString().Trim();
            }

            var report = new SafetyReport
            {
                ReportId = reportId,
                UserId = user.Id, // Always save UserId so it appears in My Reports. Identity is hidden from staff if IsAnonymous is true.
                IncidentCategory = finalCategory,
                IncidentLocation = model.IncidentLocation,
                IncidentDate = model.IncidentDate,
                IncidentTime = model.IncidentTime,
                Description = model.Description,
                IsAnonymous = model.IsAnonymous,
                Severity = model.Severity,
                ImmediateDanger = model.ImmediateDanger,
                MedicalAssistanceRequired = model.MedicalAssistanceRequired,
                SecurityAssistanceRequired = model.SecurityAssistanceRequired,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Source = model.Source
            };

            _context.SafetyReports.Add(report);

            var history = new SafetyReportStatusHistory
            {
                SafetyReport = report,
                OldStatus = "",
                NewStatus = "Pending",
                ChangedBy = "System",
                ChangedAt = DateTime.UtcNow,
                Comment = "Report created"
            };

            _context.SafetyReportStatusHistories.Add(history);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Success), new { id = reportId });
        }

        // GET: /IncidentReport/Success
        [HttpGet]
        public async Task<IActionResult> Success(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            var report = await _context.SafetyReports.FirstOrDefaultAsync(r => r.ReportId == id);
            
            if (report == null)
            {
                return NotFound();
            }

            // Prevent viewing others' non-anonymous reports unless staff/admin
            if (!report.IsAnonymous && report.UserId != user?.Id)
            {
                return Unauthorized();
            }

            return View(report);
        }

        // GET: /IncidentReport/MyReports
        [HttpGet]
        public async Task<IActionResult> MyReports(SafetyReportFilterViewModel filter, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            int pageSize = 10;
            var query = _context.SafetyReports.Where(r => r.UserId == user.Id).AsQueryable();

            // Total counts for dashboard cards
            var totalReports = await query.CountAsync();
            var pendingCount = await query.CountAsync(r => r.Status == "Pending");
            var reviewCount = await query.CountAsync(r => r.Status == "Under Review");
            var investigationCount = await query.CountAsync(r => r.Status == "Investigation");
            var resolvedCount = await query.CountAsync(r => r.Status == "Resolved");
            var rejectedCount = await query.CountAsync(r => r.Status == "Rejected");
            var actionTakenCount = await query.CountAsync(r => r.Status == "Action Taken");

            // Apply Filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                query = query.Where(r => r.ReportId.ToLower().Contains(search) 
                    || r.IncidentCategory.ToLower().Contains(search)
                    || r.IncidentLocation.ToLower().Contains(search)
                    || r.Description.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "All")
            {
                query = query.Where(r => r.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.Category) && filter.Category != "All")
            {
                query = query.Where(r => r.IncidentCategory == filter.Category);
            }

            if (!string.IsNullOrWhiteSpace(filter.Severity) && filter.Severity != "All")
            {
                query = query.Where(r => r.Severity == filter.Severity);
            }

            if (!string.IsNullOrWhiteSpace(filter.DateRange) && filter.DateRange != "All Time")
            {
                DateTime cutoff = DateTime.UtcNow;
                switch (filter.DateRange)
                {
                    case "Today": cutoff = DateTime.UtcNow.Date; break;
                    case "Last 7 Days": cutoff = DateTime.UtcNow.AddDays(-7); break;
                    case "Last 30 Days": cutoff = DateTime.UtcNow.AddDays(-30); break;
                    case "Last 3 Months": cutoff = DateTime.UtcNow.AddMonths(-3); break;
                }
                query = query.Where(r => r.CreatedAt >= cutoff);
            }

            // Apply Sorting
            query = filter.SortBy switch
            {
                "Oldest" => query.OrderBy(r => r.CreatedAt),
                "Recently Updated" => query.OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt),
                "Severity" => query.OrderByDescending(r => r.Severity).ThenByDescending(r => r.CreatedAt),
                "Status" => query.OrderBy(r => r.Status).ThenByDescending(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt) // Default: Newest
            };

            var filteredCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(filteredCount / (double)pageSize);
            var reports = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new SafetyReportListViewModel
            {
                Reports = reports,
                TotalReports = totalReports,
                PendingCount = pendingCount,
                UnderReviewCount = reviewCount,
                InvestigationCount = investigationCount,
                ResolvedCount = resolvedCount,
                RejectedCount = rejectedCount,
                ActionTakenCount = actionTakenCount,
                Filter = filter,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // GET: /IncidentReport/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var report = await _context.SafetyReports
                .Include(r => r.StatusHistory)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null)
            {
                return NotFound();
            }

            // Must be owner, staff/admin, or the report must be anonymous
            if (!User.IsInRole("Staff") && !User.IsInRole("Admin") && !report.IsAnonymous && report.UserId != user.Id)
            {
                return Unauthorized();
            }

            var viewModel = new SafetyReportDetailsViewModel
            {
                Report = report,
                StatusHistory = report.StatusHistory.OrderBy(h => h.ChangedAt).ToList()
            };

            return View(viewModel);
        }

        // POST: /IncidentReport/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var report = await _context.SafetyReports.FirstOrDefaultAsync(r => r.ReportId == id);
            
            if (report == null)
            {
                return NotFound();
            }

            // If UserId doesn't match (or is null for old anonymous reports), they can't withdraw it
            if (report.UserId != user.Id)
            {
                TempData["ErrorMessage"] = "You cannot withdraw this report because it is an old anonymous report that does not have your User ID attached to it.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            if (report.Status != "Pending")
            {
                TempData["ErrorMessage"] = "You can only withdraw reports that are in Pending status.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            report.Status = "Withdrawn";
            report.UpdatedAt = DateTime.UtcNow;

            var history = new SafetyReportStatusHistory
            {
                SafetyReportId = report.SafetyReportId,
                OldStatus = "Pending",
                NewStatus = "Withdrawn",
                ChangedBy = user.UserName ?? "Student",
                ChangedAt = DateTime.UtcNow,
                Comment = "Report withdrawn by the student."
            };

            _context.SafetyReportStatusHistories.Add(history);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Report successfully withdrawn.";
            return RedirectToAction(nameof(MyReports));
        }
    }
}
