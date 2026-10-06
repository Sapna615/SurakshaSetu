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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (User.HasClaim("Permission", "ManageCounseling"))
                return RedirectToAction("Dashboard", "Counselor");
            if (User.HasClaim("Permission", "ManageHostel"))
                return RedirectToAction("Dashboard", "Warden");
            if (User.HasClaim("Permission", "ReadOnlyAnalytics"))
                return RedirectToAction("Dashboard", "Management");

            var reports = await _context.SafetyReports.AsNoTracking().ToListAsync();
            var sosAlerts = await _context.SOSAlerts.AsNoTracking().ToListAsync();
            var safetyAlerts = await _context.SafetyAlerts.AsNoTracking().ToListAsync();
            var allUsers = await _userManager.Users.AsNoTracking().ToListAsync();
            
            // Get user counts by role. This requires a query with the AspNetUserRoles join
            var studentRoleId = await _context.Roles.Where(r => r.Name == "Student").Select(r => r.Id).FirstOrDefaultAsync();
            var counselorRoleId = await _context.Roles.Where(r => r.Name == "Counselor").Select(r => r.Id).FirstOrDefaultAsync();
            var securityRoleId = await _context.Roles.Where(r => r.Name == "SecurityOfficer").Select(r => r.Id).FirstOrDefaultAsync();
            
            var userRoles = await _context.UserRoles.AsNoTracking().ToListAsync();
            
            int studentCount = userRoles.Count(ur => ur.RoleId == studentRoleId);
            int counselorCount = userRoles.Count(ur => ur.RoleId == counselorRoleId);
            int securityCount = userRoles.Count(ur => ur.RoleId == securityRoleId);
            
            int activeUserCount = allUsers.Count; // Assuming all users are active currently as there's no IsActive flag yet

            var viewModel = new AdminDashboardViewModel
            {
                AdminName = user.FullName ?? user.UserName ?? "Admin",
                
                TotalStudents = studentCount,
                TotalCounselors = counselorCount,
                TotalSecurityStaff = securityCount,
                TotalActiveUsers = activeUserCount,
                
                TotalReports = reports.Count,
                OpenCases = reports.Count(r => r.Status != "Resolved" && r.Status != "Cancelled"),
                ResolvedCases = reports.Count(r => r.Status == "Resolved"),
                
                ActiveSOS = sosAlerts.Count(s => s.Status != "Resolved" && s.Status != "Cancelled"),
                ActiveSafetyAlerts = safetyAlerts.Count(s => s.IsActive && (s.ExpiresAt == null || s.ExpiresAt > System.DateTime.UtcNow)),
                
                RecentReports = reports.OrderByDescending(r => r.CreatedAt).Take(5).ToList(),
                RecentSOSRequests = sosAlerts.OrderByDescending(s => s.CreatedAt).Take(5).ToList(),
                RecentSafetyAlerts = safetyAlerts.OrderByDescending(s => s.PublishedAt).Take(5).ToList(),
                RecentUserRegistrations = allUsers.OrderByDescending(u => u.Id).Take(5).ToList() // Identity doesn't store CreatedAt by default, ordering by Id roughly approximates
            };

            ViewData["UserName"] = viewModel.AdminName;
            ViewData["RoleName"] = "Admin";

            return View(viewModel);
        }

        // ==========================================
        // PHASE 2: USER MANAGEMENT
        // ==========================================

        public async Task<IActionResult> Users(string? searchQuery, string? roleFilter, string? departmentFilter, string? statusFilter)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                usersQuery = usersQuery.Where(u => 
                    (u.FullName != null && u.FullName.ToLower().Contains(searchLower)) || 
                    (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                    (u.StudentId != null && u.StudentId.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrWhiteSpace(departmentFilter))
            {
                usersQuery = usersQuery.Where(u => u.Department == departmentFilter);
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                bool wantActive = statusFilter == "Active";
                var now = System.DateTimeOffset.UtcNow;
                if (wantActive)
                {
                    usersQuery = usersQuery.Where(u => u.LockoutEnd == null || u.LockoutEnd <= now);
                }
                else
                {
                    usersQuery = usersQuery.Where(u => u.LockoutEnd != null && u.LockoutEnd > now);
                }
            }

            var allUsers = await usersQuery.AsNoTracking().ToListAsync();
            
            // To prevent N+1 queries, load user roles directly via EF
            var userRolesMap = new Dictionary<string, List<string>>();
            
            var allUserRoles = await _context.UserRoles.ToListAsync();
            var allRoles = await _context.Roles.ToListAsync();
            
            foreach (var ur in allUserRoles)
            {
                if (!userRolesMap.ContainsKey(ur.UserId))
                {
                    userRolesMap[ur.UserId] = new List<string>();
                }
                var roleName = allRoles.FirstOrDefault(r => r.Id == ur.RoleId)?.Name;
                if (roleName != null)
                {
                    userRolesMap[ur.UserId].Add(roleName);
                }
            }

            var userViewModels = allUsers.Select(u => new AdminUserListViewModel
            {
                Id = u.Id,
                FullName = u.FullName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                StudentId = u.StudentId,
                Department = u.Department,
                Roles = userRolesMap.ContainsKey(u.Id) ? userRolesMap[u.Id] : new List<string>(),
                IsActive = u.LockoutEnd == null || u.LockoutEnd <= System.DateTimeOffset.UtcNow
            }).ToList();

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                userViewModels = userViewModels.Where(u => u.Roles.Contains(roleFilter)).ToList();
            }

            // Prepare filters
            var departments = await _userManager.Users
                .Where(u => u.Department != null)
                .Select(u => u.Department!)
                .Distinct()
                .ToListAsync();

            var viewModel = new AdminUsersPageViewModel
            {
                Users = userViewModels,
                SearchQuery = searchQuery,
                RoleFilter = roleFilter,
                DepartmentFilter = departmentFilter,
                StatusFilter = statusFilter,
                AvailableRoles = allRoles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = r.Name, Text = r.Name }).ToList(),
                AvailableDepartments = departments.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = d, Text = d }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var viewModel = new AdminUserEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                StudentId = user.StudentId,
                Department = user.Department,
                Year = user.Year,
                IsActive = user.LockoutEnd == null || user.LockoutEnd <= System.DateTimeOffset.UtcNow,
                CurrentRoles = userRoles.ToList(),
                SelectedRoles = userRoles.ToList(),
                AvailableRoles = allRoles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = r.Name, Text = r.Name }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(AdminUserEditViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            // Repopulate dropdowns in case of validation failure
            var allRoles = await _roleManager.Roles.ToListAsync();
            model.AvailableRoles = allRoles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = r.Name, Text = r.Name }).ToList();

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // Do not allow the admin to demote themselves inadvertently
            if (user.Id == currentUser!.Id && !model.SelectedRoles.Contains("Admin"))
            {
                ModelState.AddModelError("", "You cannot remove your own Admin role.");
                return View(model);
            }
            
            if (user.Id == currentUser!.Id && !model.IsActive)
            {
                ModelState.AddModelError("", "You cannot deactivate your own account.");
                return View(model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.StudentId = model.StudentId;
            user.Department = model.Department;
            user.Year = model.Year;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // Update Roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();

            if (rolesToAdd.Any()) await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (rolesToRemove.Any()) await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            // Update Status
            bool currentlyActive = user.LockoutEnd == null || user.LockoutEnd <= System.DateTimeOffset.UtcNow;
            if (model.IsActive != currentlyActive)
            {
                if (model.IsActive)
                {
                    await _userManager.SetLockoutEndDateAsync(user, null);
                }
                else
                {
                    await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.MaxValue);
                }
            }

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (id == currentUser!.Id)
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            bool currentlyActive = user.LockoutEnd == null || user.LockoutEnd <= System.DateTimeOffset.UtcNow;
            
            if (currentlyActive)
            {
                await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.MaxValue);
                TempData["SuccessMessage"] = $"User {user.FullName} deactivated.";
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["SuccessMessage"] = $"User {user.FullName} activated.";
            }

            return RedirectToAction(nameof(Users));
        }

        // ==========================================
        // PHASE 3: CASE MANAGEMENT
        // ==========================================

        public async Task<IActionResult> Cases(string? searchQuery, string? categoryFilter, string? severityFilter, string? statusFilter, string? assignedFilter, System.DateTime? startDate, System.DateTime? endDate)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var casesQuery = _context.SafetyReports.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                casesQuery = casesQuery.Where(c => 
                    (c.ReportId != null && c.ReportId.ToLower().Contains(searchLower)) ||
                    (c.IncidentLocation != null && c.IncidentLocation.ToLower().Contains(searchLower)) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrWhiteSpace(categoryFilter)) casesQuery = casesQuery.Where(c => c.IncidentCategory == categoryFilter);
            if (!string.IsNullOrWhiteSpace(severityFilter)) casesQuery = casesQuery.Where(c => c.Severity == severityFilter);
            if (!string.IsNullOrWhiteSpace(statusFilter)) casesQuery = casesQuery.Where(c => c.Status == statusFilter);
            
            if (!string.IsNullOrWhiteSpace(assignedFilter)) 
            {
                if (assignedFilter == "Unassigned") casesQuery = casesQuery.Where(c => string.IsNullOrEmpty(c.AssignedTo));
                else casesQuery = casesQuery.Where(c => c.AssignedTo == assignedFilter);
            }

            if (startDate.HasValue) casesQuery = casesQuery.Where(c => c.CreatedAt >= startDate.Value);
            if (endDate.HasValue) 
            {
                var endOfDay = endDate.Value.AddDays(1).AddTicks(-1);
                casesQuery = casesQuery.Where(c => c.CreatedAt <= endOfDay);
            }

            var cases = await casesQuery.OrderByDescending(c => c.CreatedAt).AsNoTracking().ToListAsync();

            var categories = await _context.IncidentCategories.Select(c => c.Name).ToListAsync();
            
            var statuses = new List<string> { "Pending", "In Progress", "Under Investigation", "Resolved", "Cancelled" };

            var viewModel = new AdminCasesPageViewModel
            {
                Cases = cases,
                SearchQuery = searchQuery,
                CategoryFilter = categoryFilter,
                SeverityFilter = severityFilter,
                StatusFilter = statusFilter,
                AssignedFilter = assignedFilter,
                StartDate = startDate,
                EndDate = endDate,
                AvailableCategories = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c, Text = c }).ToList(),
                AvailableStatuses = statuses.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = s, Text = s }).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ViewCase(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var report = await _context.SafetyReports.FirstOrDefaultAsync(r => r.SafetyReportId == id);
            if (report == null) return NotFound();

            var timeline = await _context.SafetyReportStatusHistories
                .Where(h => h.SafetyReportId == id)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            // Get security staff for reassignment
            var securityRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "SecurityOfficer");
            List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> staffList = new();
            if (securityRole != null)
            {
                var securityUserRoles = await _context.UserRoles.Where(ur => ur.RoleId == securityRole.Id).ToListAsync();
                var userIds = securityUserRoles.Select(ur => ur.UserId).ToList();
                var securityUsers = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                
                staffList = securityUsers.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = u.FullName,
                    Text = $"{u.FullName} ({u.Department ?? "Security"})"
                }).ToList();
            }

            var viewModel = new AdminCaseDetailViewModel
            {
                Report = report,
                Timeline = timeline,
                SecurityStaff = staffList
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignCase(int reportId, string? newAssignee)
        {
            var report = await _context.SafetyReports.FirstOrDefaultAsync(r => r.SafetyReportId == reportId);
            if (report == null) return NotFound();

            report.AssignedTo = string.IsNullOrWhiteSpace(newAssignee) ? null : newAssignee;
            report.UpdatedAt = System.DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Case reassigned successfully.";
            return RedirectToAction(nameof(ViewCase), new { id = reportId });
        }

        // ==========================================
        // PHASE 4: SAFETY ALERTS
        // ==========================================

        public async Task<IActionResult> SafetyAlerts(string? searchQuery, string? statusFilter, string? categoryFilter)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var alertsQuery = _context.SafetyAlerts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                alertsQuery = alertsQuery.Where(a => 
                    (a.Title != null && a.Title.ToLower().Contains(searchLower)) ||
                    (a.Message != null && a.Message.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrWhiteSpace(categoryFilter))
            {
                alertsQuery = alertsQuery.Where(a => a.Category == categoryFilter);
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                bool wantActive = statusFilter == "Active";
                var now = System.DateTime.UtcNow;
                if (wantActive)
                {
                    alertsQuery = alertsQuery.Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > now));
                }
                else
                {
                    alertsQuery = alertsQuery.Where(a => !a.IsActive || (a.ExpiresAt != null && a.ExpiresAt <= now));
                }
            }

            var alerts = await alertsQuery.OrderByDescending(a => a.PublishedAt).AsNoTracking().ToListAsync();

            var categories = await _context.SafetyAlerts.Select(a => a.Category).Distinct().ToListAsync();

            var viewModel = new AdminSafetyAlertsPageViewModel
            {
                Alerts = alerts,
                SearchQuery = searchQuery,
                StatusFilter = statusFilter,
                CategoryFilter = categoryFilter,
                AvailableCategories = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c, Text = c }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSafetyAlert()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var viewModel = new AdminSafetyAlertFormViewModel
            {
                IsActive = true
            };
            return View("~/Views/Admin/SafetyAlertForm.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSafetyAlert(AdminSafetyAlertFormViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            if (!ModelState.IsValid) return View("~/Views/Admin/SafetyAlertForm.cshtml", model);

            var alert = new SafetyAlert
            {
                Title = model.Title,
                Message = model.Message,
                Priority = model.Priority,
                Category = model.Category,
                Location = model.Location ?? string.Empty,
                RecommendedAction = model.RecommendedAction ?? string.Empty,
                ExpiresAt = model.ExpiresAt,
                IsActive = model.IsActive,
                PublishedAt = System.DateTime.UtcNow,
                PublishedBy = currentUser?.FullName ?? "System Admin"
            };

            _context.SafetyAlerts.Add(alert);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Safety alert published successfully.";
            return RedirectToAction(nameof(SafetyAlerts));
        }

        [HttpGet]
        public async Task<IActionResult> EditSafetyAlert(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var alert = await _context.SafetyAlerts.FirstOrDefaultAsync(a => a.SafetyAlertId == id);
            if (alert == null) return NotFound();

            var viewModel = new AdminSafetyAlertFormViewModel
            {
                SafetyAlertId = alert.SafetyAlertId,
                Title = alert.Title,
                Message = alert.Message,
                Priority = alert.Priority,
                Category = alert.Category,
                Location = alert.Location,
                RecommendedAction = alert.RecommendedAction,
                ExpiresAt = alert.ExpiresAt,
                IsActive = alert.IsActive
            };

            return View("~/Views/Admin/SafetyAlertForm.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSafetyAlert(AdminSafetyAlertFormViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            if (!ModelState.IsValid) return View("~/Views/Admin/SafetyAlertForm.cshtml", model);

            var alert = await _context.SafetyAlerts.FirstOrDefaultAsync(a => a.SafetyAlertId == model.SafetyAlertId);
            if (alert == null) return NotFound();

            alert.Title = model.Title;
            alert.Message = model.Message;
            alert.Priority = model.Priority;
            alert.Category = model.Category;
            alert.Location = model.Location ?? string.Empty;
            alert.RecommendedAction = model.RecommendedAction ?? string.Empty;
            alert.ExpiresAt = model.ExpiresAt;
            alert.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Safety alert updated successfully.";
            return RedirectToAction(nameof(SafetyAlerts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSafetyAlert(int id)
        {
            var alert = await _context.SafetyAlerts.FirstOrDefaultAsync(a => a.SafetyAlertId == id);
            if (alert == null) return NotFound();

            alert.IsActive = !alert.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = alert.IsActive ? "Alert activated." : "Alert deactivated.";
            return RedirectToAction(nameof(SafetyAlerts));
        }

        // ==========================================
        // PHASE 5: EMERGENCY CONTACTS
        // ==========================================

        public async Task<IActionResult> EmergencyContacts(string? searchQuery, string? categoryFilter)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var contactsQuery = _context.EmergencyContacts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                contactsQuery = contactsQuery.Where(c => 
                    (c.Name != null && c.Name.ToLower().Contains(searchLower)) ||
                    (c.Department != null && c.Department.ToLower().Contains(searchLower)) ||
                    (c.Role != null && c.Role.ToLower().Contains(searchLower)) ||
                    (c.PhoneNumber != null && c.PhoneNumber.ToLower().Contains(searchLower)) ||
                    (c.Email != null && c.Email.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrWhiteSpace(categoryFilter))
            {
                contactsQuery = contactsQuery.Where(c => c.Category == categoryFilter);
            }

            var contacts = await contactsQuery.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).AsNoTracking().ToListAsync();

            var categories = await _context.EmergencyContacts.Select(c => c.Category).Distinct().ToListAsync();

            var viewModel = new AdminEmergencyContactsPageViewModel
            {
                Contacts = contacts,
                SearchQuery = searchQuery,
                CategoryFilter = categoryFilter,
                AvailableCategories = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c, Text = c }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateEmergencyContact()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var viewModel = new AdminEmergencyContactFormViewModel
            {
                IsActive = true,
                Is24x7 = false,
                CanCall = true,
                IsEmergency = false,
                ShowInQuickGuide = true,
                DisplayOrder = 0
            };
            return View("~/Views/Admin/EmergencyContactForm.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmergencyContact(AdminEmergencyContactFormViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            if (!ModelState.IsValid) return View("~/Views/Admin/EmergencyContactForm.cshtml", model);

            var contact = new EmergencyContact
            {
                Name = model.Name,
                Department = model.Department ?? string.Empty,
                Role = model.Role ?? string.Empty,
                Category = model.Category,
                PhoneNumber = model.PhoneNumber ?? string.Empty,
                Email = model.Email ?? string.Empty,
                Location = model.Location ?? string.Empty,
                Description = model.Description ?? string.Empty,
                Availability = model.Availability ?? string.Empty,
                Is24x7 = model.Is24x7,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                WebsiteUrl = model.WebsiteUrl ?? string.Empty,
                CanCall = model.CanCall,
                IsEmergency = model.IsEmergency,
                ShowInQuickGuide = model.ShowInQuickGuide,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedAt = System.DateTime.UtcNow
            };

            _context.EmergencyContacts.Add(contact);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Emergency contact created successfully.";
            return RedirectToAction(nameof(EmergencyContacts));
        }

        [HttpGet]
        public async Task<IActionResult> EditEmergencyContact(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var contact = await _context.EmergencyContacts.FirstOrDefaultAsync(c => c.EmergencyContactId == id);
            if (contact == null) return NotFound();

            var viewModel = new AdminEmergencyContactFormViewModel
            {
                EmergencyContactId = contact.EmergencyContactId,
                Name = contact.Name,
                Department = contact.Department,
                Role = contact.Role,
                Category = contact.Category,
                PhoneNumber = contact.PhoneNumber,
                Email = contact.Email,
                Location = contact.Location,
                Description = contact.Description,
                Availability = contact.Availability,
                Is24x7 = contact.Is24x7,
                Latitude = contact.Latitude,
                Longitude = contact.Longitude,
                WebsiteUrl = contact.WebsiteUrl,
                CanCall = contact.CanCall,
                IsEmergency = contact.IsEmergency,
                ShowInQuickGuide = contact.ShowInQuickGuide,
                IsActive = contact.IsActive,
                DisplayOrder = contact.DisplayOrder
            };

            return View("~/Views/Admin/EmergencyContactForm.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmergencyContact(AdminEmergencyContactFormViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            if (!ModelState.IsValid) return View("~/Views/Admin/EmergencyContactForm.cshtml", model);

            var contact = await _context.EmergencyContacts.FirstOrDefaultAsync(c => c.EmergencyContactId == model.EmergencyContactId);
            if (contact == null) return NotFound();

            contact.Name = model.Name;
            contact.Department = model.Department ?? string.Empty;
            contact.Role = model.Role ?? string.Empty;
            contact.Category = model.Category;
            contact.PhoneNumber = model.PhoneNumber ?? string.Empty;
            contact.Email = model.Email ?? string.Empty;
            contact.Location = model.Location ?? string.Empty;
            contact.Description = model.Description ?? string.Empty;
            contact.Availability = model.Availability ?? string.Empty;
            contact.Is24x7 = model.Is24x7;
            contact.Latitude = model.Latitude;
            contact.Longitude = model.Longitude;
            contact.WebsiteUrl = model.WebsiteUrl ?? string.Empty;
            contact.CanCall = model.CanCall;
            contact.IsEmergency = model.IsEmergency;
            contact.ShowInQuickGuide = model.ShowInQuickGuide;
            contact.IsActive = model.IsActive;
            contact.DisplayOrder = model.DisplayOrder;
            contact.UpdatedAt = System.DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Emergency contact updated successfully.";
            return RedirectToAction(nameof(EmergencyContacts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEmergencyContact(int id)
        {
            var contact = await _context.EmergencyContacts.FirstOrDefaultAsync(c => c.EmergencyContactId == id);
            if (contact == null) return NotFound();

            contact.IsActive = !contact.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = contact.IsActive ? "Contact activated." : "Contact deactivated.";
            return RedirectToAction(nameof(EmergencyContacts));
        }

        // ==========================================
        // PHASE 6: LOCATIONS
        // ==========================================

        public async Task<IActionResult> Locations(string? searchQuery, string? typeFilter)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var locQuery = _context.CampusLocations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                locQuery = locQuery.Where(l => 
                    (l.Name != null && l.Name.ToLower().Contains(searchLower)) ||
                    (l.Description != null && l.Description.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                locQuery = locQuery.Where(l => l.LocationType == typeFilter);
            }

            var locations = await locQuery.OrderBy(l => l.Name).AsNoTracking().ToListAsync();

            var types = await _context.CampusLocations.Select(l => l.LocationType).Distinct().ToListAsync();

            var viewModel = new AdminLocationsPageViewModel
            {
                Locations = locations,
                SearchQuery = searchQuery,
                TypeFilter = typeFilter,
                AvailableTypes = types.Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = t, Text = t }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateLocation()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var viewModel = new AdminLocationFormViewModel
            {
                IsActive = true
            };
            return View("~/Views/Admin/LocationForm.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(AdminLocationFormViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            if (!ModelState.IsValid) return View("~/Views/Admin/LocationForm.cshtml", model);

            var location = new CampusLocation
            {
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                LocationType = model.LocationType,
                IsActive = model.IsActive
            };

            _context.CampusLocations.Add(location);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Location created successfully.";
            return RedirectToAction(nameof(Locations));
        }

        [HttpGet]
        public async Task<IActionResult> EditLocation(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var location = await _context.CampusLocations.FirstOrDefaultAsync(l => l.CampusLocationId == id);
            if (location == null) return NotFound();

            var viewModel = new AdminLocationFormViewModel
            {
                CampusLocationId = location.CampusLocationId,
                Name = location.Name,
                Description = location.Description,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                LocationType = location.LocationType,
                IsActive = location.IsActive
            };

            return View("~/Views/Admin/LocationForm.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLocation(AdminLocationFormViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            if (!ModelState.IsValid) return View("~/Views/Admin/LocationForm.cshtml", model);

            var location = await _context.CampusLocations.FirstOrDefaultAsync(l => l.CampusLocationId == model.CampusLocationId);
            if (location == null) return NotFound();

            location.Name = model.Name;
            location.Description = model.Description ?? string.Empty;
            location.Latitude = model.Latitude;
            location.Longitude = model.Longitude;
            location.LocationType = model.LocationType;
            location.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Location updated successfully.";
            return RedirectToAction(nameof(Locations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLocation(int id)
        {
            var location = await _context.CampusLocations.FirstOrDefaultAsync(l => l.CampusLocationId == id);
            if (location == null) return NotFound();

            location.IsActive = !location.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = location.IsActive ? "Location activated." : "Location deactivated.";
            return RedirectToAction(nameof(Locations));
        }

        // ==========================================
        // PHASE 7: CATEGORIES & CONFIGURATION
        // ==========================================

        public async Task<IActionResult> Categories(string? searchQuery)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var catQuery = _context.IncidentCategories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                catQuery = catQuery.Where(c => 
                    (c.Name != null && c.Name.ToLower().Contains(searchLower)) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchLower))
                );
            }

            var categories = await catQuery.OrderBy(c => c.Name).AsNoTracking().ToListAsync();

            var viewModel = new AdminCategoriesPageViewModel
            {
                Categories = categories,
                SearchQuery = searchQuery
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCategory()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var viewModel = new AdminCategoryFormViewModel
            {
                IsActive = true
            };
            return View("~/Views/Admin/CategoryForm.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(AdminCategoryFormViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            if (!ModelState.IsValid) return View("~/Views/Admin/CategoryForm.cshtml", model);

            var category = new IncidentCategory
            {
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                IsActive = model.IsActive
            };

            _context.IncidentCategories.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.IncidentCategoryId == id);
            if (category == null) return NotFound();

            var viewModel = new AdminCategoryFormViewModel
            {
                IncidentCategoryId = category.IncidentCategoryId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };

            return View("~/Views/Admin/CategoryForm.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(AdminCategoryFormViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            if (!ModelState.IsValid) return View("~/Views/Admin/CategoryForm.cshtml", model);

            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.IncidentCategoryId == model.IncidentCategoryId);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.Description = model.Description ?? string.Empty;
            category.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategory(int id)
        {
            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.IncidentCategoryId == id);
            if (category == null) return NotFound();

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = category.IsActive ? "Category activated." : "Category deactivated.";
            return RedirectToAction(nameof(Categories));
        }

        // ==========================================
        // PHASE 8: AUDIT LOGS
        // ==========================================

        public async Task<IActionResult> AuditLogs(string? searchQuery, DateTime? startDate, DateTime? endDate)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = currentUser?.FullName ?? "Admin";

            var logsQuery = _context.AuditLogs.Include(l => l.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                logsQuery = logsQuery.Where(l => 
                    (l.Action != null && l.Action.ToLower().Contains(searchLower)) ||
                    (l.Details != null && l.Details.ToLower().Contains(searchLower)) ||
                    (l.User != null && l.User.FullName.ToLower().Contains(searchLower)) ||
                    (l.User != null && l.User.Email.ToLower().Contains(searchLower))
                );
            }

            if (startDate.HasValue)
            {
                logsQuery = logsQuery.Where(l => l.Timestamp >= startDate.Value.ToUniversalTime());
            }

            if (endDate.HasValue)
            {
                // Add 1 day to include the entire end date
                var endDay = endDate.Value.AddDays(1).ToUniversalTime();
                logsQuery = logsQuery.Where(l => l.Timestamp < endDay);
            }

            var logs = await logsQuery.OrderByDescending(l => l.Timestamp).Take(500).AsNoTracking().ToListAsync();

            var viewModel = new AdminAuditLogsPageViewModel
            {
                Logs = logs,
                SearchQuery = searchQuery,
                StartDate = startDate,
                EndDate = endDate
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Admin";

            var roles = await _userManager.GetRolesAsync(user);

            var model = new SecurityProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Department = user.Department ?? "Administration",
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
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Admin";
            return View("~/Views/Security/ChangePassword.cshtml", new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            ViewData["UserName"] = user.FullName ?? user.UserName ?? "Admin";

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
        
        [HttpGet]
        public IActionResult Counseling()
        {
            // Requirement: Integrate existing Counseling Center functionality using current CounselorController
            return RedirectToAction("Dashboard", "Counselor");
        }

        [HttpGet]
        public IActionResult SensitiveCases()
        {
            // Requirement: Sensitive Cases as an authorized filtered view over existing SafetyReport system
            return RedirectToAction("Cases", "Counselor");
        }

        [HttpGet]
        public IActionResult Analytics()
        {
            // Requirement: Integrate existing Analytics functionality
            return RedirectToAction("Dashboard", "Management");
        }
    }
}
