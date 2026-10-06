using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using SurakshaSetu.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SurakshaSetu.Controllers
{
    [Authorize]
    [Route("Safety")]
    public class SafetyMapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SafetyMapController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Map")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("api/mapdata")]
        public async Task<IActionResult> GetMapData()
        {
            // Use AsNoTracking() for performance since this is a read-only map query
            var locations = await _context.CampusLocations
                .AsNoTracking()
                .Where(l => l.IsActive && l.Latitude.HasValue && l.Longitude.HasValue)
                .Select(l => new MapLocationDto
                {
                    LocationId = l.CampusLocationId,
                    Name = l.Name,
                    Description = l.Description ?? string.Empty,
                    Latitude = l.Latitude ?? 0,
                    Longitude = l.Longitude ?? 0,
                    LocationType = l.LocationType,
                    // Secure aggregation: We ONLY count incidents (last 30 days), never exposing sensitive data
                    RecentIncidentsCount = _context.SafetyReports.Count(i => i.IncidentLocation == l.Name && i.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                    InfrastructureIssuesCount = _context.SafetyReports.Count(i => i.IncidentLocation == l.Name && i.IncidentCategory == "Infrastructure Issue"),
                    ActiveAlertsCount = _context.SafetyAlerts.Count(a => a.Location == l.Name && a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow))
                })
                .ToListAsync();

            return Json(locations);
        }
    }
}
