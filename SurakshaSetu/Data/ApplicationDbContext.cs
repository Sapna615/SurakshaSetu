using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Models;

namespace SurakshaSetu.Data
{
    // Inherit from IdentityDbContext to include all Identity tables (Users, Roles, Claims, etc.)
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CampusLocation> CampusLocations { get; set; }
        public DbSet<IncidentCategory> IncidentCategories { get; set; }
        public DbSet<SafetyAlert> SafetyAlerts { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<SOSAlert> SOSAlerts { get; set; }
        public DbSet<SafetyReport> SafetyReports { get; set; }
        public DbSet<SafetyReportStatusHistory> SafetyReportStatusHistories { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // We MUST call the base method first, otherwise Identity tables won't generate properly!
            base.OnModelCreating(builder);
        }
    }
}
