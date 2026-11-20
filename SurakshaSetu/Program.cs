using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using SurakshaSetu.Models;
using SurakshaSetu.Services;

if (args.Contains("db_check")) {
    SurakshaSetu.DbCheck.Run(args);
    return;
}

if (args.Contains("export_db")) {
    SurakshaSetu.DatabaseViewerExporter.GenerateHtmlViewer();
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=SurakshaSetu.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (defaultConn.Contains(".db", StringComparison.OrdinalIgnoreCase) || 
        defaultConn.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) && !defaultConn.Contains("Server=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(defaultConn);
    }
    else
    {
        options.UseSqlServer(defaultConn);
    }
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Identity UI requires Razor Pages
builder.Services.AddRazorPages();

// Register dev email sender (logs reset links to console)
builder.Services.AddSingleton<IEmailSender, DevEmailSender>();

// Add Policy-Based Authorization for Specialist Roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManageWomensSafety", policy => policy.RequireAssertion(c => c.User.HasClaim("Permission", "ManageWomensSafety") || c.User.IsInRole("Admin") || c.User.IsInRole("SuperAdmin")));
    options.AddPolicy("ManageCounseling", policy => policy.RequireAssertion(c => c.User.HasClaim("Permission", "ManageCounseling") || c.User.IsInRole("Admin") || c.User.IsInRole("SuperAdmin")));
    options.AddPolicy("ManageHostel", policy => policy.RequireAssertion(c => c.User.HasClaim("Permission", "ManageHostel") || c.User.IsInRole("Admin") || c.User.IsInRole("SuperAdmin")));
    options.AddPolicy("ManageGrievance", policy => policy.RequireAssertion(c => c.User.HasClaim("Permission", "ManageGrievance") || c.User.IsInRole("Admin") || c.User.IsInRole("SuperAdmin")));
    options.AddPolicy("ReadOnlyAnalytics", policy => policy.RequireAssertion(c => c.User.HasClaim("Permission", "ReadOnlyAnalytics") || c.User.IsInRole("Admin") || c.User.IsInRole("SuperAdmin")));
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
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        // Ensure database schema is created and all tables exist
        await dbContext.Database.EnsureCreatedAsync();

        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing or seeding the database.");
    }
}

app.Run();
