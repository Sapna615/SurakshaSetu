using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SurakshaSetu.Data;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer("Server=localhost,1433;Database=SurakshaSetuDb;User Id=sa;Password=SurakshaSetu@2026!;TrustServerCertificate=True;");
var context = new ApplicationDbContext(optionsBuilder.Options);
var count = context.EmergencyContacts.Count();
Console.WriteLine($"DB Count: {count}");
foreach(var c in context.EmergencyContacts.ToList()) {
    Console.WriteLine($"Name: {c.Name}, Category: {c.Category}, IsActive: {c.IsActive}");
}
