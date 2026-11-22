using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SurakshaSetu.Data;

namespace SurakshaSetu
{
    class DbCheck
    {
        public static void Run(string[] args)
        {
            Console.WriteLine("DB Check Start");
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=SurakshaSetuDb;User Id=sa;Password=SurakshaSetu@2026!;TrustServerCertificate=True;");
            
            using var context = new ApplicationDbContext(optionsBuilder.Options);
            var alertsCount = context.SafetyAlerts.Count();
            Console.WriteLine($"Total SafetyAlerts in DB: {alertsCount}");
            foreach (var a in context.SafetyAlerts.ToList())
            {
                Console.WriteLine($"- Alert: {a.Title}, IsActive: {a.IsActive}, ExpiresAt: {a.ExpiresAt}");
            }
        }
    }
}
