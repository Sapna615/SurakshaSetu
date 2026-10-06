using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using SurakshaSetu.Models;
using System.Linq;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer("Server=localhost;Database=SurakshaSetuDb;User Id=sa;Password=Your_Password123;Encrypt=False;");
var context = new ApplicationDbContext(optionsBuilder.Options);

var user = context.Users.FirstOrDefault(u => u.Email == "security@surakshasetu.edu");
if(user != null) {
    var roles = context.UserRoles.Where(r => r.UserId == user.Id).ToList();
    foreach(var r in roles) {
        var roleName = context.Roles.FirstOrDefault(ro => ro.Id == r.RoleId)?.Name;
        Console.WriteLine(roleName);
    }
}
