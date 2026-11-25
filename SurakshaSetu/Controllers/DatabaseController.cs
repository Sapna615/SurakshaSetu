using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;

namespace SurakshaSetu.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DatabaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/database/viewer")]
        [HttpGet("/DatabaseViewer.html")]
        public async Task<IActionResult> Viewer()
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();
            var roles = await _context.Roles.AsNoTracking().ToListAsync();
            var userRoles = await _context.UserRoles.AsNoTracking().ToListAsync();
            var reports = await _context.SafetyReports.Include(r => r.Feedback).AsNoTracking().OrderByDescending(r => r.CreatedAt).ToListAsync();
            var locations = await _context.CampusLocations.AsNoTracking().ToListAsync();
            var contacts = await _context.EmergencyContacts.AsNoTracking().OrderBy(c => c.DisplayOrder).ToListAsync();
            var feedbacks = await _context.CaseFeedbacks.AsNoTracking().OrderByDescending(f => f.SubmittedAt).ToListAsync();
            var categories = await _context.IncidentCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.Append(@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>SurakshaSetu Live Database Visualizer</title>
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css'>
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css'>
    <style>
        body { background: linear-gradient(180deg, #f0f4f9 0%, #e9eef5 100%); font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; padding: 30px; min-height: 100vh; }
        .card { border-radius: 14px; border: 1px solid #e2e8f0; box-shadow: 0 4px 15px rgba(0,0,0,0.04); margin-bottom: 30px; overflow: hidden; }
        .card-header { background: #0f172a; color: #fff; font-weight: 700; font-size: 1.05rem; padding: 14px 20px; }
        .table { margin-bottom: 0; }
        .badge { font-weight: 600; }
        .live-pulse { width: 10px; height: 10px; background-color: #22c55e; border-radius: 50%; display: inline-block; box-shadow: 0 0 0 rgba(34, 197, 94, 0.4); animation: pulse 2s infinite; }
        @keyframes pulse { 0% { box-shadow: 0 0 0 0 rgba(34, 197, 94, 0.7); } 70% { box-shadow: 0 0 0 8px rgba(34, 197, 94, 0); } 100% { box-shadow: 0 0 0 0 rgba(34, 197, 94, 0); } }
    </style>
</head>
<body>
    <div class='container-fluid' style='max-width: 1400px;'>
        <div class='d-flex flex-wrap justify-content-between align-items-center mb-4 pb-2 border-bottom'>
            <div>
                <h2 class='fw-bold text-dark mb-1 d-flex align-items-center gap-2'>
                    <span>🗄️ SurakshaSetu Live Database Visualizer</span>
                </h2>
                <p class='text-muted mb-0'>Real-time dynamic query directly from <code>SurakshaSetuDb</code> &bull; Refreshes instantly on page reload</p>
            </div>
            <div class='d-flex align-items-center gap-2 mt-2 mt-md-0'>
                <span class='badge bg-white text-success border border-success-subtle p-2 fs-6 d-flex align-items-center gap-2 shadow-sm'>
                    <span class='live-pulse'></span> Live Connected
                </span>
                <button onclick='window.location.reload()' class='btn btn-primary btn-sm fw-bold px-3 py-2 shadow-sm'>
                    <i class='bi bi-arrow-clockwise me-1'></i> Refresh Data
                </button>
            </div>
        </div>

        <!-- 1. USERS TABLE -->
        <div class='card'>
            <div class='card-header d-flex justify-content-between align-items-center'>
                <span><i class='bi bi-people-fill me-2 text-primary'></i> Table: AspNetUsers (Registered Accounts)</span>
                <span class='badge bg-primary px-3 py-1'>" + users.Count + @" Total Users</span>
            </div>
            <div class='table-responsive'>
                <table class='table table-hover table-striped align-middle'>
                    <thead class='table-light'>
                        <tr>
                            <th>Full Name</th>
                            <th>Email</th>
                            <th>Role</th>
                            <th>Student ID</th>
                            <th>Department</th>
                            <th>Phone</th>
                        </tr>
                    </thead>
                    <tbody>");

            foreach (var u in users)
            {
                var roleId = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.RoleId;
                var roleName = roles.FirstOrDefault(r => r.Id == roleId)?.Name ?? "User";
                var roleBadge = roleName switch
                {
                    "SuperAdmin" => "<span class='badge bg-danger'>SuperAdmin</span>",
                    "Admin" => "<span class='badge bg-warning text-dark'>Admin</span>",
                    "SecurityOfficer" => "<span class='badge bg-info text-dark'>SecurityOfficer</span>",
                    "Student" => "<span class='badge bg-primary'>Student</span>",
                    _ => $"<span class='badge bg-secondary'>{roleName}</span>"
                };

                sb.Append($@"<tr>
                    <td><strong>{u.FullName ?? "—"}</strong></td>
                    <td><code>{u.Email}</code></td>
                    <td>{roleBadge}</td>
                    <td>{(string.IsNullOrEmpty(u.StudentId) ? "<span class='text-muted'>—</span>" : u.StudentId)}</td>
                    <td>{(string.IsNullOrEmpty(u.Department) ? "<span class='text-muted'>—</span>" : u.Department)}</td>
                    <td>{(string.IsNullOrEmpty(u.PhoneNumber) ? "<span class='text-muted'>—</span>" : u.PhoneNumber)}</td>
                </tr>");
            }

            sb.Append(@"</tbody></table></div></div>");

            // 2. INCIDENT REPORTS & SOS ALERTS
            sb.Append(@"<div class='card'>
                <div class='card-header d-flex justify-content-between align-items-center'>
                    <span><i class='bi bi-shield-exclamation me-2 text-danger'></i> Table: SafetyReports (Incidents & SOS Alerts)</span>
                    <span class='badge bg-danger px-3 py-1'>" + reports.Count + @" Total Reports</span>
                </div>
                <div class='table-responsive'>
                    <table class='table table-hover table-striped align-middle'>
                        <thead class='table-light'>
                            <tr>
                                <th>Report ID</th>
                                <th>Category</th>
                                <th>Severity</th>
                                <th>Status</th>
                                <th>Location</th>
                                <th>Assigned To</th>
                                <th>Created (UTC)</th>
                                <th>Feedback / Rating</th>
                            </tr>
                        </thead>
                        <tbody>");

            foreach (var r in reports)
            {
                var sevBadge = r.Severity switch
                {
                    "Critical" => "<span class='badge bg-danger'>Critical</span>",
                    "High" => "<span class='badge bg-warning text-dark'>High</span>",
                    "Medium" => "<span class='badge bg-info text-dark'>Medium</span>",
                    _ => "<span class='badge bg-secondary'>Low</span>"
                };

                var statusBadge = r.Status switch
                {
                    "Resolved" => "<span class='badge bg-success'>Resolved</span>",
                    "In Progress" => "<span class='badge bg-primary'>In Progress</span>",
                    "Assigned" => "<span class='badge bg-info text-dark'>Assigned</span>",
                    "Cancelled" => "<span class='badge bg-secondary'>Cancelled</span>",
                    _ => $"<span class='badge bg-light text-dark border'>{r.Status}</span>"
                };

                var feedbackText = r.Feedback != null 
                    ? $"<span class='text-warning fw-bold'>★ {r.Feedback.Rating}/5</span> <small class='text-muted d-block'>\"{r.Feedback.Comments}\"</small>" 
                    : "<span class='text-muted small'>—</span>";

                sb.Append($@"<tr>
                    <td><strong>{r.ReportId}</strong></td>
                    <td>{r.IncidentCategory}</td>
                    <td>{sevBadge}</td>
                    <td>{statusBadge}</td>
                    <td>{r.IncidentLocation}</td>
                    <td>{(string.IsNullOrEmpty(r.AssignedTo) ? "<span class='text-muted'>Unassigned</span>" : $"<code>{r.AssignedTo}</code>")}</td>
                    <td><small>{r.CreatedAt:yyyy-MM-dd HH:mm}</small></td>
                    <td>{feedbackText}</td>
                </tr>");
            }

            sb.Append(@"</tbody></table></div></div>");

            // 3. STUDENT FEEDBACKS TABLE
            sb.Append(@"<div class='card'>
                <div class='card-header d-flex justify-content-between align-items-center'>
                    <span><i class='bi bi-star-fill me-2 text-warning'></i> Table: CaseFeedbacks (Student Ratings & Reviews)</span>
                    <span class='badge bg-warning text-dark px-3 py-1'>" + feedbacks.Count + @" Feedbacks</span>
                </div>
                <div class='table-responsive'>
                    <table class='table table-hover table-striped align-middle'>
                        <thead class='table-light'>
                            <tr>
                                <th>Case Report ID</th>
                                <th>Rating</th>
                                <th>Satisfied?</th>
                                <th>Student Comments</th>
                                <th>Submitted At</th>
                            </tr>
                        </thead>
                        <tbody>");

            if (!feedbacks.Any())
            {
                sb.Append("<tr><td colspan='5' class='text-center py-3 text-muted'>No feedbacks submitted yet.</td></tr>");
            }
            else
            {
                foreach (var f in feedbacks)
                {
                    sb.Append($@"<tr>
                        <td><strong>{f.ReportId}</strong></td>
                        <td><span class='text-warning fw-bold fs-6'>{'★' * f.Rating}</span> ({f.Rating}/5)</td>
                        <td>{(f.WasResolvedSatisfactorily ? "<span class='badge bg-success'>Yes</span>" : "<span class='badge bg-danger'>No</span>")}</td>
                        <td>{f.Comments ?? "<span class='text-muted'>—</span>"}</td>
                        <td><small>{f.SubmittedAt:yyyy-MM-dd HH:mm}</small></td>
                    </tr>");
                }
            }

            sb.Append(@"</tbody></table></div></div>");

            // 4. EMERGENCY CONTACTS TABLE
            sb.Append(@"<div class='card'>
                <div class='card-header d-flex justify-content-between align-items-center'>
                    <span><i class='bi bi-telephone-fill me-2 text-success'></i> Table: EmergencyContacts (Campus Directory)</span>
                    <span class='badge bg-success px-3 py-1'>" + contacts.Count + @" Contacts</span>
                </div>
                <div class='table-responsive'>
                    <table class='table table-hover table-striped align-middle'>
                        <thead class='table-light'>
                            <tr>
                                <th>Order</th>
                                <th>Name</th>
                                <th>Department & Role</th>
                                <th>Phone</th>
                                <th>Email</th>
                                <th>24x7 Support</th>
                            </tr>
                        </thead>
                        <tbody>");

            foreach (var c in contacts)
            {
                sb.Append($@"<tr>
                    <td>{c.DisplayOrder}</td>
                    <td><strong>{c.Name}</strong></td>
                    <td>{c.Department} ({c.Role})</td>
                    <td><code>{c.PhoneNumber}</code></td>
                    <td>{(string.IsNullOrEmpty(c.Email) ? "—" : c.Email)}</td>
                    <td>{(c.Is24x7 ? "<span class='badge bg-success'>24x7 Active</span>" : "<span class='badge bg-secondary'>Standard Hours</span>")}</td>
                </tr>");
            }

            sb.Append(@"</tbody></table></div></div>");

            // 5. INCIDENT CATEGORIES TABLE
            sb.Append(@"<div class='card'>
                <div class='card-header d-flex justify-content-between align-items-center bg-primary'>
                    <span><i class='bi bi-tags-fill me-2'></i> Table: IncidentCategories (Incident Classifications)</span>
                    <span class='badge bg-white text-primary px-3 py-1'>" + categories.Count + @" Categories</span>
                </div>
                <div class='table-responsive'>
                    <table class='table table-hover table-striped align-middle'>
                        <thead class='table-light'>
                            <tr>
                                <th>ID</th>
                                <th>Category Name</th>
                                <th>Status</th>
                                <th>Description</th>
                            </tr>
                        </thead>
                        <tbody>");

            foreach (var cat in categories)
            {
                sb.Append($@"<tr>
                    <td>{cat.IncidentCategoryId}</td>
                    <td><strong>{cat.Name}</strong></td>
                    <td>{(cat.IsActive ? "<span class='badge bg-success'>Active</span>" : "<span class='badge bg-secondary'>Inactive</span>")}</td>
                    <td>{cat.Description ?? "<span class='text-muted'>—</span>"}</td>
                </tr>");
            }

            sb.Append(@"</tbody></table></div></div>");

            // 6. CAMPUS LOCATIONS TABLE
            sb.Append(@"<div class='card'>
                <div class='card-header d-flex justify-content-between align-items-center bg-info text-dark'>
                    <span class='fw-bold'><i class='bi bi-geo-alt-fill me-2 text-primary'></i> Table: CampusLocations (Campus Geo-Points)</span>
                    <span class='badge bg-dark text-white px-3 py-1'>" + locations.Count + @" Locations</span>
                </div>
                <div class='table-responsive'>
                    <table class='table table-hover table-striped align-middle'>
                        <thead class='table-light'>
                            <tr>
                                <th>ID</th>
                                <th>Location Name</th>
                                <th>Type</th>
                                <th>Latitude</th>
                                <th>Longitude</th>
                                <th>Status</th>
                            </tr>
                        </thead>
                        <tbody>");

            foreach (var loc in locations)
            {
                sb.Append($@"<tr>
                    <td>{loc.CampusLocationId}</td>
                    <td><strong>{loc.Name}</strong></td>
                    <td><span class='badge bg-light text-dark border'>{loc.LocationType}</span></td>
                    <td>{(loc.Latitude.HasValue ? loc.Latitude.Value.ToString("F4") : "—")}</td>
                    <td>{(loc.Longitude.HasValue ? loc.Longitude.Value.ToString("F4") : "—")}</td>
                    <td>{(loc.IsActive ? "<span class='badge bg-success'>Active</span>" : "<span class='badge bg-secondary'>Inactive</span>")}</td>
                </tr>");
            }

            sb.Append(@"</tbody></table></div></div>");
            sb.Append(@"</div></body></html>");

            return Content(sb.ToString(), "text/html");
        }
    }
}
