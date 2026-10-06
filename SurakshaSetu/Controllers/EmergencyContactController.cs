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
    public class EmergencyContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmergencyContactController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // EMERGENCY CONTACT PAGE
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var activeSOS = await _context.SOSAlerts
                .Where(s =>
                    s.UserId == user.Id &&
                    s.Status != "Cancelled" &&
                    s.Status != "Resolved")
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            var contacts = await _context.EmergencyContacts
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Category)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var categories = contacts
                .Select(c => c.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            var preferredOrder = new[]
            {
                "Emergency",
                "Security",
                "Medical",
                "Women Safety",
                "Hostel",
                "Counselling",
                "Grievance",
                "Cyber Safety",
                "Student Welfare",
                "Transport"
            };

            categories = categories
                .OrderBy(c =>
                {
                    int index = Array.IndexOf(preferredOrder, c);

                    return index == -1
                        ? 999
                        : index;
                })
                .ThenBy(c => c)
                .ToList();

            var viewModel = new EmergencyContactsViewModel
            {
                Contacts = contacts,
                Categories = categories,
                ActiveSOS = activeSOS,

                EmergencyContacts = contacts
                    .Where(c =>
                        c.Category == "Emergency" ||
                        c.IsEmergency)
                    .ToList(),

                SecurityContacts = contacts
                    .Where(c => c.Category == "Security")
                    .ToList(),

                MedicalContacts = contacts
                    .Where(c => c.Category == "Medical")
                    .ToList(),

                WomenSafetyContacts = contacts
                    .Where(c => c.Category == "Women Safety")
                    .ToList(),

                HostelContacts = contacts
                    .Where(c => c.Category == "Hostel")
                    .ToList(),

                CounsellingContacts = contacts
                    .Where(c => c.Category == "Counselling")
                    .ToList(),

                GrievanceContacts = contacts
                    .Where(c => c.Category == "Grievance")
                    .ToList(),

                CyberSafetyContacts = contacts
                    .Where(c => c.Category == "Cyber Safety")
                    .ToList(),

                StudentWelfareContacts = contacts
                    .Where(c => c.Category == "Student Welfare")
                    .ToList(),

                TransportContacts = contacts
                    .Where(c => c.Category == "Transport")
                    .ToList()
            };

            return View(viewModel);
        }


        // ============================================================
        // SEARCH CONTACTS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Search(
            string? search,
            string? category)
        {
            var query = _context.EmergencyContacts
                .Where(c => c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.Name.Contains(search) ||
                    c.Department.Contains(search) ||
                    c.Role.Contains(search) ||
                    c.Category.Contains(search) ||
                    c.Description.Contains(search) ||
                    c.Location.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category) &&
                category != "All")
            {
                query = query.Where(c =>
                    c.Category == category);
            }

            var contacts = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return Json(contacts);
        }


        // ============================================================
        // CONTACT DETAILS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var contact = await _context.EmergencyContacts
                .FirstOrDefaultAsync(c =>
                    c.EmergencyContactId == id &&
                    c.IsActive);

            if (contact == null)
            {
                return NotFound();
            }

            return Json(contact);
        }


        // ============================================================
        // MAP CONTACTS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> MapContacts()
        {
            var contacts = await _context.EmergencyContacts
                .Where(c =>
                    c.IsActive &&
                    c.Latitude.HasValue &&
                    c.Longitude.HasValue)
                .Select(c => new
                {
                    id = c.EmergencyContactId,
                    name = c.Name,
                    category = c.Category,
                    department = c.Department,
                    location = c.Location,
                    latitude = c.Latitude,
                    longitude = c.Longitude,
                    is24x7 = c.Is24x7
                })
                .ToListAsync();

            return Json(contacts);
        }


        // ============================================================
        // ACTIVATE SOS
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateSOS()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var existingSOS = await _context.SOSAlerts
                .Where(s =>
                    s.UserId == user.Id &&
                    s.Status != "Cancelled" &&
                    s.Status != "Resolved")
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (existingSOS != null)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "You already have an active emergency request."
                });
            }

            var sos = new SOSAlert
            {
                UserId = user.Id,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.SOSAlerts.Add(sos);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message =
                    "Emergency SOS activated successfully."
            });
        }


        // ============================================================
        // CANCEL SOS
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSOS()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var sos = await _context.SOSAlerts
                .Where(s =>
                    s.UserId == user.Id &&
                    s.Status != "Cancelled" &&
                    s.Status != "Resolved")
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (sos == null)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "No active SOS request found."
                });
            }

            sos.Status = "Cancelled";

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message =
                    "SOS request cancelled."
            });
        }


        // ============================================================
        // ACTIVE SOS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ActiveSOS()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var sos = await _context.SOSAlerts
                .Where(s =>
                    s.UserId == user.Id &&
                    s.Status != "Cancelled" &&
                    s.Status != "Resolved")
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (sos == null)
            {
                return Json(new
                {
                    active = false
                });
            }

            return Json(new
            {
                active = true,
                status = sos.Status,
                createdAt = sos.CreatedAt
            });
        }


        // ============================================================
        // SUBMIT SAFETY REPORT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSafetyReport(
            string incidentCategory,
            string incidentLocation,
            DateTime incidentDate,
            string incidentTime,
            string description,
            bool isAnonymous)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // ------------------------------
            // VALIDATION
            // ------------------------------

            if (string.IsNullOrWhiteSpace(incidentCategory))
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Please select an incident category."
                });
            }

            if (string.IsNullOrWhiteSpace(incidentLocation))
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Please enter the incident location."
                });
            }

            if (string.IsNullOrWhiteSpace(incidentTime))
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Please enter the incident time."
                });
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Please describe what happened."
                });
            }

            if (incidentDate.Date > DateTime.Today)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Incident date cannot be in the future."
                });
            }


            // ------------------------------
            // GENERATE REPORT ID
            // ------------------------------

            string reportId;

            do
            {
                reportId =
                    "SS-" +
                    DateTime.Now.ToString("yyMMdd") +
                    "-" +
                    Random.Shared.Next(1000, 9999);

            }
            while (await _context.SafetyReports
                .AnyAsync(r => r.ReportId == reportId));


            // ------------------------------
            // CREATE REPORT
            // ------------------------------

            var report = new SafetyReport
            {
                ReportId = reportId,

                UserId = isAnonymous
                    ? null
                    : user.Id,

                IncidentCategory =
                    incidentCategory.Trim(),

                IncidentLocation =
                    incidentLocation.Trim(),

                IncidentDate =
                    incidentDate,

                IncidentTime =
                    incidentTime.Trim(),

                Description =
                    description.Trim(),

                IsAnonymous =
                    isAnonymous,

                Status =
                    "Pending",

                CreatedAt =
                    DateTime.UtcNow
            };


            _context.SafetyReports.Add(report);

            await _context.SaveChangesAsync();


            // ------------------------------
            // SUCCESS RESPONSE
            // ------------------------------

            return Json(new
            {
                success = true,

                reportId = reportId,

                status = "Pending",

                anonymous = isAnonymous,

                message =
                    $"Safety report {reportId} has been successfully submitted."
            });
        }


        // ============================================================
        // GET MY REPORTS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> MyReports()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var reports = await _context.SafetyReports
                .Where(r =>
                    r.UserId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    reportId = r.ReportId,
                    category = r.IncidentCategory,
                    location = r.IncidentLocation,
                    date = r.IncidentDate,
                    time = r.IncidentTime,
                    status = r.Status,
                    createdAt = r.CreatedAt
                })
                .ToListAsync();

            return Json(reports);
        }
    }
}
