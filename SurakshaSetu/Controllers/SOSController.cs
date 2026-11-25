using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using SurakshaSetu.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SurakshaSetu.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SOSController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SOSController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class SOSRequestDto
        {
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public string LocationDescription { get; set; }
        }

        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] SOSRequestDto dto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                // Generate a fresh unique SOS report every time the user triggers SOS
                var randomNum = new Random().Next(1000, 9999);
                var sos = new SafetyReport
                {
                    UserId = user.Id,
                    ReportId = "SOS-" + DateTime.Now.ToString("yyMMdd") + "-" + randomNum,
                    IncidentLocation = string.IsNullOrEmpty(dto?.LocationDescription) ? "Campus Location (GPS Active)" : dto.LocationDescription,
                    Status = "Assigned",
                    ImmediateDanger = true,
                    Severity = "Critical",
                    IncidentCategory = "Emergency SOS",
                    IncidentDate = DateTime.Today,
                    IncidentTime = DateTime.Now.ToString("HH:mm"),
                    Description = "URGENT: Emergency SOS Activated via Student Safety Button.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SafetyReports.Add(sos);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, sos = new { requestId = sos.ReportId, status = sos.Status, locationDescription = sos.IncidentLocation, createdAt = sos.CreatedAt } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, inner = ex.InnerException?.Message, trace = ex.StackTrace });
            }
        }

        [HttpPost("cancel/{requestId}")]
        public async Task<IActionResult> Cancel(string requestId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var sos = await _context.SafetyReports.FirstOrDefaultAsync(s => s.ReportId == requestId && s.UserId == user.Id);
            if (sos == null) return NotFound();

            sos.Status = "Cancelled";
            sos.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var activeSOS = await _context.SafetyReports
                .Where(s => s.UserId == user.Id && s.ImmediateDanger && s.Status != "Cancelled" && s.Status != "Resolved")
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (activeSOS == null) return Ok(new { active = false });
            
            return Ok(new { active = true, sos = new { requestId = activeSOS.ReportId, status = activeSOS.Status, locationDescription = activeSOS.IncidentLocation, createdAt = activeSOS.CreatedAt } });
        }

        [HttpGet("active")]
        [Authorize(Roles = "SecurityOfficer,Admin,SuperAdmin")]
        public async Task<IActionResult> GetActiveSOS()
        {
            var activeSOSList = await _context.SafetyReports
                .Where(s => s.ImmediateDanger && s.Status != "Cancelled" && s.Status != "Resolved")
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new {
                    id = s.SafetyReportId,
                    reportId = s.ReportId,
                    location = s.IncidentLocation,
                    time = s.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    status = s.Status
                })
                .ToListAsync();

            // Also retrieve recent cancelled SOS requests from the last 2 hours so security is explicitly notified of cancellations
            var cutoff = DateTime.UtcNow.AddHours(-2);
            var recentCancelledList = await _context.SafetyReports
                .Where(s => s.ImmediateDanger && s.Status == "Cancelled" && s.UpdatedAt >= cutoff)
                .OrderByDescending(s => s.UpdatedAt)
                .Take(5)
                .Select(s => new {
                    id = s.SafetyReportId,
                    reportId = s.ReportId,
                    location = s.IncidentLocation,
                    time = (s.UpdatedAt ?? s.CreatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    status = "Cancelled"
                })
                .ToListAsync();

            return Ok(new { count = activeSOSList.Count, alerts = activeSOSList, cancelled = recentCancelledList });
        }
    }
}
