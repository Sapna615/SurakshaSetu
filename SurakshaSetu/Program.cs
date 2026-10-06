using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using SurakshaSetu.Models;

if (args.Contains("db_check")) {
    SurakshaSetu.DbCheck.Run(args);
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Identity UI requires Razor Pages
builder.Services.AddRazorPages();

// Add Policy-Based Authorization for Specialist Roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManageWomensSafety", policy => policy.RequireAssertion(c => c.User.HasClaim("Permission", "ManageWomensSafety") || c.User.IsInRole("Admin")));
    options.AddPolicy("ManageCounseling", policy => policy.RequireAssertion(c => c.User.HasClaim("Permission", "ManageCounseling") || c.User.IsInRole("Admin")));
    options.AddPolicy("ManageHostel", policy => policy.RequireAssertion(c => c.User.HasClaim("Permission", "ManageHostel") || c.User.IsInRole("Admin")));
    options.AddPolicy("ManageGrievance", policy => policy.RequireAssertion(c => c.User.HasClaim("Permission", "ManageGrievance") || c.User.IsInRole("Admin")));
    options.AddPolicy("ReadOnlyAnalytics", policy => policy.RequireAssertion(c => c.User.HasClaim("Permission", "ReadOnlyAnalytics") || c.User.IsInRole("Admin")));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Map Identity Razor Pages
app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
