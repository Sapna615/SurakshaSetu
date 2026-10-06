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
            var count = context.SafetyReports.Count();
            Console.WriteLine($"Total Safety Reports: {count}");
            
            var incidents = context.SafetyReports.ToList();
            foreach(var inc in incidents) {
                Console.WriteLine($"- {inc.ReportId} ({inc.IncidentCategory}) Status: {inc.Status}");
            }
        }
    }
}
