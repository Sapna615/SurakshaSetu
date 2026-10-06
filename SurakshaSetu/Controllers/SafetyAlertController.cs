using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SurakshaSetu.Controllers
{
    [Authorize(Roles = "Student")]
    public class SafetyAlertController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SafetyAlertController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // GET:
        // /SafetyAlert
        public async Task<IActionResult> Index()
        {
            var alerts = await _context.SafetyAlerts
                .Where(a =>
                    a.IsActive &&
                    (
                        a.ExpiresAt == null ||
                        a.ExpiresAt > DateTime.UtcNow
                    )
                )
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();

            return View(alerts);
        }

        // GET:
        // /SafetyAlert/Details/5
        public async Task<IActionResult> Details(
            int id)
        {
            var alert =
                await _context.SafetyAlerts
                .FirstOrDefaultAsync(
                    a =>
                        a.SafetyAlertId == id &&
                        a.IsActive
                );

            if (alert == null)
            {
                return NotFound();
            }

            return Json(alert);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(
            string? priority)
        {
            var query = _context.SafetyAlerts
                .Where(a => a.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(priority) &&
                priority != "all")
            {
                query = query.Where(
                    a => a.Priority == priority
                );
            }

            var alerts = await query
                .OrderByDescending(
                    a => a.PublishedAt
                )
                .ToListAsync();

            return PartialView(
                "_AlertCards",
                alerts
            );
        }
    }
}
