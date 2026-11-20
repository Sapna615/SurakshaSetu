using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;

namespace SurakshaSetu
{
    public class DatabaseViewerExporter
    {
        public static void GenerateHtmlViewer()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=SurakshaSetuDb;User Id=sa;Password=SurakshaSetu@2026!;TrustServerCertificate=True;");

            using var context = new ApplicationDbContext(optionsBuilder.Options);

            var users = context.Users.ToList();
            var roles = context.Roles.ToList();
            var userRoles = context.UserRoles.ToList();
            var reports = context.SafetyReports.OrderByDescending(r => r.CreatedAt).ToList();
            var locations = context.CampusLocations.ToList();
            var contacts = context.EmergencyContacts.OrderBy(c => c.DisplayOrder).ToList();
            var categories = context.IncidentCategories.ToList();

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <title>SurakshaSetu Database Visual Explorer</title>
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css'>
    <style>
        body { background: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; padding: 30px; }
        .card { border-radius: 12px; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); margin-bottom: 30px; }
        .card-header { background: #1e293b; color: #fff; font-weight: bold; font-size: 1.1rem; border-radius: 12px 12px 0 0 !important; }
        .table { margin-bottom: 0; }
        .badge { font-weight: 600; }
    </style>
</head>
<body>
    <div class='container-fluid'>
        <div class='d-flex justify-content-between align-items-center mb-4'>
            <div>
                <h2 class='fw-bold text-dark'>🗄️ SurakshaSetu Database Visualizer</h2>
                <p class='text-muted'>Live Database View from <code>SurakshaSetuDb</code> on SQL Server (port 1433)</p>
            </div>
            <span class='badge bg-success p-2 fs-6'>Connected: localhost,1433</span>
        </div>

        <!-- 1. USERS TABLE -->
        <div class='card'>
            <div class='card-header d-flex justify-content-between'>
                <span>👥 Table: AspNetUsers (Registered Accounts)</span>
                <span class='badge bg-primary'>" + users.Count + @" Total</span>
            </div>
            <div class='table-responsive'>
                <table class='table table-hover table-striped align-middle'>
                    <thead class='table-light'>
                        <tr>
                            <th>Id</th>
                            <th>Full Name</th>
                            <th>Email / UserName</th>
                            <th>Role</th>
                            <th>Phone</th>
                            <th>Email Confirmed</th>
                        </tr>
                    </thead>
                    <tbody>");

            foreach (var u in users)
            {
                var roleId = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.RoleId;
                var roleName = roles.FirstOrDefault(r => r.Id == roleId)?.Name ?? "No Role";
                sb.Append($@"<tr>
                    <td><small class='text-muted'>{u.Id}</small></td>
                    <td><strong>{u.FullName}</strong></td>
                    <td>{u.Email}</td>
                    <td><span class='badge bg-info text-dark'>{roleName}</span></td>
                    <td>{u.PhoneNumber ?? "N/A"}</td>
                    <td>{(u.EmailConfirmed ? "<span class='badge bg-success'>Yes</span>" : "<span class='badge bg-warning'>No</span>")}</td>
                </tr>");
            }

            sb.Append(@"</tbody></table></div></div>");

            // 2. SAFETY REPORTS
            sb.Append(@"<div class='card'>
            <div class='card-header d-flex justify-content-between bg-danger'>
                <span>🚨 Table: SafetyReports (Incidents & SOS Alerts)</span>
                <span class='badge bg-light text-dark'>" + reports.Count + @" Total</span>
            </div>
            <div class='table-responsive'>
                <table class='table table-hover table-striped align-middle'>
                    <thead class='table-light'>
                        <tr>
                            <th>Report ID</th>
                            <th>Category</th>
                            <th>Location</th>
                            <th>Severity</th>
                            <th>Status</th>
                            <th>Immediate Danger</th>
                            <th>Assigned To</th>
                            <th>Created At</th>
                        </tr>
                    </thead>
                    <tbody>");

            foreach (var r in reports)
            {
                var statusBadge = r.Status == "Pending" || r.Status == "Created" ? "bg-danger" : r.Status == "In Progress" || r.Status == "Assigned" ? "bg-warning text-dark" : "bg-success";
                sb.Append($@"<tr>
                    <td><strong>{r.ReportId}</strong></td>
                    <td><span class='badge bg-secondary'>{r.IncidentCategory}</span></td>
                    <td>{r.IncidentLocation}</td>
                    <td><span class='badge {(r.Severity == "Critical" ? "bg-danger" : "bg-warning text-dark")}'>{r.Severity}</span></td>
                    <td><span class='badge {statusBadge}'>{r.Status}</span></td>
                    <td>{(r.ImmediateDanger ? "<span class='badge bg-danger'>YES (SOS)</span>" : "<span class='badge bg-light text-muted border'>No</span>")}</td>
                    <td>{(string.IsNullOrEmpty(r.AssignedTo) ? "<span class='text-muted'>Unassigned</span>" : r.AssignedTo)}</td>
                    <td>{r.CreatedAt:yyyy-MM-dd HH:mm:ss}</td>
                </tr>");
            }

            sb.Append(@"</tbody></table></div></div>");

            // 3. CAMPUS LOCATIONS
            sb.Append(@"<div class='card'>
            <div class='card-header d-flex justify-content-between bg-primary'>
                <span>📍 Table: CampusLocations (Safety Map Coordinates)</span>
                <span class='badge bg-light text-dark'>" + locations.Count + @" Total</span>
            </div>
            <div class='table-responsive'>
                <table class='table table-hover table-striped align-middle'>
                    <thead class='table-light'>
                        <tr>
                            <th>ID</th>
                            <th>Name</th>
                            <th>Type</th>
                            <th>Latitude</th>
                            <th>Longitude</th>
                            <th>Description</th>
                        </tr>
                    </thead>
                    <tbody>");

            foreach (var l in locations)
            {
                sb.Append($@"<tr>
                    <td>{l.CampusLocationId}</td>
                    <td><strong>{l.Name}</strong></td>
                    <td><span class='badge bg-info text-dark'>{l.LocationType}</span></td>
                    <td><code>{l.Latitude}</code></td>
                    <td><code>{l.Longitude}</code></td>
                    <td>{l.Description}</td>
                </tr>");
            }

            sb.Append(@"</tbody></table></div></div>");

            // 4. EMERGENCY CONTACTS
            sb.Append(@"<div class='card'>
            <div class='card-header d-flex justify-content-between bg-success'>
                <span>📞 Table: EmergencyContacts (Directory)</span>
                <span class='badge bg-light text-dark'>" + contacts.Count + @" Total</span>
            </div>
            <div class='table-responsive'>
                <table class='table table-hover table-striped align-middle'>
                    <thead class='table-light'>
                        <tr>
                            <th>Order</th>
                            <th>Name</th>
                            <th>Department</th>
                            <th>Phone</th>
                            <th>Email</th>
                            <th>24x7</th>
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
                    <td>{c.Email}</td>
                    <td>{(c.Is24x7 ? "<span class='badge bg-success'>24x7</span>" : "Standard")}</td>
                </tr>");
            }

            sb.Append(@"</tbody></table></div></div>");

            sb.Append(@"</div></body></html>");

            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseViewer.html");
            File.WriteAllText(outputPath, sb.ToString());
            Console.WriteLine($"SUCCESS: Database Viewer exported to: {outputPath}");
        }
    }
}
