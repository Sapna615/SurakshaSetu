using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SurakshaSetu.Models;

namespace SurakshaSetu.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Role-based Routing: If a student is logged in, don't show them the public landing page.
        // Send them directly to their personalized safety dashboard.
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("SuperAdmin")) return RedirectToAction("Dashboard", "SuperAdmin");
            if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
            if (User.IsInRole("SecurityOfficer")) return RedirectToAction("Dashboard", "Security");
            if (User.IsInRole("Student")) return RedirectToAction("Dashboard", "Student");
        }
        
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
