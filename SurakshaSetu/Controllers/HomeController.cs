using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using SurakshaSetu.Models;

namespace SurakshaSetu.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Role-based Routing: If a student is logged in, don't show them the public landing page.
        // Send them directly to their personalized safety dashboard.
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
            if (User.IsInRole("SecurityOfficer")) return RedirectToAction("Dashboard", "Security");
            if (User.IsInRole("Student")) return RedirectToAction("Dashboard", "Student");
        }

        // Supply dynamic 24/7 contacts, campus zones, and categories to the landing page
        ViewBag.EmergencyContacts = await _context.EmergencyContacts
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Take(6)
            .ToListAsync();

        ViewBag.CampusLocations = await _context.CampusLocations
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .Take(8)
            .ToListAsync();

        ViewBag.IncidentCategories = await _context.IncidentCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
