using Microsoft.AspNetCore.Identity;
using SurakshaSetu.Models;

namespace SurakshaSetu.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // We get the services we need to manage roles, users, and the database
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Seed Roles
            string[] roleNames = { "SuperAdmin", "Admin", "SecurityOfficer", "Student" };
            foreach (var roleName in roleNames)
            {
                // If the role doesn't exist, create it
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Default SuperAdmin User
            string adminEmail = "admin@surakshasetu.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                // Create the user with a strong default password
                var result = await userManager.CreateAsync(adminUser, "Admin@2026!");
                if (result.Succeeded)
                {
                    // Assign the SuperAdmin role to this user
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                }
            }

            // 3. Seed Incident Categories
            if (!context.IncidentCategories.Any())
            {
                context.IncidentCategories.AddRange(
                    new IncidentCategory { Name = "Harassment", Description = "Verbal or physical harassment" },
                    new IncidentCategory { Name = "Theft", Description = "Stolen property or belongings" },
                    new IncidentCategory { Name = "Medical Emergency", Description = "Health related emergencies" },
                    new IncidentCategory { Name = "Infrastructure Issue", Description = "Broken lights, missing cameras, etc." }
                );
                await context.SaveChangesAsync();
            }

            // Seed Test Student
            await CreateTestUserAsync(userManager, "student@surakshasetu.edu", "Sapna Student", "Student");

            // Seed Test Security Officer
            await CreateTestUserAsync(userManager, "security@surakshasetu.edu", "Officer Singh", "SecurityOfficer");
            
            // Force upgrade if they manually registered earlier as Student
            var secUser = await userManager.FindByEmailAsync("security@surakshasetu.edu");
            if (secUser != null && !await userManager.IsInRoleAsync(secUser, "SecurityOfficer"))
            {
                await userManager.RemoveFromRoleAsync(secUser, "Student");
                await userManager.AddToRoleAsync(secUser, "SecurityOfficer");
            }

            // Seed Test Admin
            await CreateTestUserAsync(userManager, "admin.test@surakshasetu.edu", "Admin Coordinator", "Admin");

            // Seed Specialist Staff (Logging in as Admin but with specific Claims)
            await CreateTestUserAsync(userManager, "womensafety@surakshasetu.edu", "Meera Sharma", "Admin", "ManageWomensSafety");
            await CreateTestUserAsync(userManager, "counselor@surakshasetu.edu", "Dr. Anil Kumar", "Admin", "ManageCounseling");
            await CreateTestUserAsync(userManager, "warden@surakshasetu.edu", "Geeta Verma", "Admin", "ManageHostel");
            await CreateTestUserAsync(userManager, "management@surakshasetu.edu", "Dean Office", "Admin", "ReadOnlyAnalytics");

            // 4. Seed Campus Locations
            var existingLocations = context.CampusLocations.ToList();
            if (existingLocations.Any())
            {
                context.CampusLocations.RemoveRange(existingLocations);
                await context.SaveChangesAsync();
            }

            if (!context.CampusLocations.Any())
            {
                context.CampusLocations.AddRange(
                    // Gates
                    new CampusLocation { Name = "LPU Main Gate", Description = "Main entrance gate of Lovely Professional University", Latitude = 31.2560, Longitude = 75.7052, LocationType = "Gate" },
                    new CampusLocation { Name = "Gate 2 (Law Gate)", Description = "Entrance near Law School", Latitude = 31.2548, Longitude = 75.7021, LocationType = "Gate" },
                    new CampusLocation { Name = "Gate 3", Description = "Student exit gate", Latitude = 31.2575, Longitude = 75.7085, LocationType = "Gate" },
                    
                    // Medical
                    new CampusLocation { Name = "Uni Hospital", Description = "LPU University Hospital and Medical Center", Latitude = 31.2540, Longitude = 75.7035, LocationType = "MedicalCenter" },
                    new CampusLocation { Name = "First Aid Clinic - BH", Description = "Clinic near Boys Hostels", Latitude = 31.2585, Longitude = 75.7075, LocationType = "MedicalCenter" },

                    // Hostels
                    new CampusLocation { Name = "Girls Hostel 1", Description = "Primary girls accommodation", Latitude = 31.2530, Longitude = 75.7020, LocationType = "Hostel" },
                    new CampusLocation { Name = "Girls Hostel 2", Description = "GH2 Accommodation", Latitude = 31.2525, Longitude = 75.7015, LocationType = "Hostel" },
                    new CampusLocation { Name = "Girls Hostel 3", Description = "GH3 Accommodation", Latitude = 31.2520, Longitude = 75.7010, LocationType = "Hostel" },
                    new CampusLocation { Name = "Boys Hostel 1", Description = "BH1 Accommodation", Latitude = 31.2570, Longitude = 75.7070, LocationType = "Hostel" },
                    new CampusLocation { Name = "Boys Hostel 2", Description = "BH2 Accommodation", Latitude = 31.2575, Longitude = 75.7075, LocationType = "Hostel" },
                    new CampusLocation { Name = "Boys Hostel 3", Description = "BH3 Accommodation", Latitude = 31.2580, Longitude = 75.7080, LocationType = "Hostel" },
                    new CampusLocation { Name = "Boys Hostel 4", Description = "BH4 Accommodation", Latitude = 31.2585, Longitude = 75.7085, LocationType = "Hostel" },

                    // Academic Buildings
                    new CampusLocation { Name = "Block 14", Description = "School of Computer Application", Latitude = 31.2545, Longitude = 75.7060, LocationType = "CampusBuilding" },
                    new CampusLocation { Name = "Block 25", Description = "School of Business", Latitude = 31.2550, Longitude = 75.7065, LocationType = "CampusBuilding" },
                    new CampusLocation { Name = "Block 32", Description = "Academic block", Latitude = 31.2555, Longitude = 75.7055, LocationType = "CampusBuilding" },
                    new CampusLocation { Name = "Block 33", Description = "Computer Science and Engineering", Latitude = 31.2560, Longitude = 75.7050, LocationType = "CampusBuilding" },
                    new CampusLocation { Name = "Block 34", Description = "Engineering Labs", Latitude = 31.2565, Longitude = 75.7045, LocationType = "CampusBuilding" },
                    new CampusLocation { Name = "Block 38", Description = "Auditorium block", Latitude = 31.2570, Longitude = 75.7040, LocationType = "CampusBuilding" },
                    new CampusLocation { Name = "Baldev Raj Mittal Unipolis", Description = "Main event arena", Latitude = 31.2558, Longitude = 75.7058, LocationType = "CampusBuilding" },

                    // Emergency Points
                    new CampusLocation { Name = "Emergency SOS Point Alpha", Description = "Blue light phone - Central", Latitude = 31.2545, Longitude = 75.7045, LocationType = "EmergencyPoint" },
                    new CampusLocation { Name = "Emergency SOS Point Beta", Description = "Blue light phone - Near BH", Latitude = 31.2582, Longitude = 75.7082, LocationType = "EmergencyPoint" },
                    new CampusLocation { Name = "Emergency SOS Point Gamma", Description = "Blue light phone - Near GH", Latitude = 31.2528, Longitude = 75.7018, LocationType = "EmergencyPoint" },

                    // Cafeterias
                    new CampusLocation { Name = "Student Center Cafeteria", Description = "Main food court", Latitude = 31.2565, Longitude = 75.7065, LocationType = "Cafeteria" },
                    new CampusLocation { Name = "Block 14 Food Court", Description = "Cafe inside Block 14", Latitude = 31.2546, Longitude = 75.7061, LocationType = "Cafeteria" },
                    new CampusLocation { Name = "Block 32 Cafe", Description = "Cafe near Block 32", Latitude = 31.2556, Longitude = 75.7056, LocationType = "Cafeteria" }
                );
                await context.SaveChangesAsync();
            }


            // 5. Seed Emergency Contacts
            // Unconditionally clear the old ones to ensure the exact matching ones are placed
            var existingContacts = context.EmergencyContacts.ToList();
            if (existingContacts.Any())
            {
                context.EmergencyContacts.RemoveRange(existingContacts);
                await context.SaveChangesAsync();
            }

            if (!context.EmergencyContacts.Any())
            {
                context.EmergencyContacts.AddRange(
                    new EmergencyContact { Name = "Campus Security", Category = "Security", Department = "Security", Role = "Command Center", Availability = "University-configured", Is24x7 = true, PhoneNumber = "+91 9876543210", Email = "", Location = "Security Gate / Control Room", Description = "24x7 campus response", DisplayOrder = 1 },
                    new EmergencyContact { Name = "Women's Safety Officer", Category = "Women's Safety", Department = "Student Welfare", Role = "Safety Officer", Availability = "Working hours", Is24x7 = false, PhoneNumber = "+91 7654321098", Email = "womensafety@surakshasetu.edu", Location = "University-configured", Description = "Safety support, guidance and incident escalation", DisplayOrder = 2 },
                    new EmergencyContact { Name = "Medical Center", Category = "Medical", Department = "Health", Role = "Medical Staff", Availability = "University-configured", Is24x7 = false, PhoneNumber = "+91 8765432109", Email = "", Location = "Medical Center", Description = "First-aid and medical assistance", DisplayOrder = 3 },
                    new EmergencyContact { Name = "Emergency Medical Support", Category = "Emergency", Department = "Health", Role = "Ambulance", Availability = "Official service / university-configured", Is24x7 = false, PhoneNumber = "108", Email = "", Location = "Emergency", Description = "Urgent medical response", DisplayOrder = 4 },
                    new EmergencyContact { Name = "Hostel Warden", Category = "Hostel", Department = "Housing", Role = "Warden", Availability = "University-configured", Is24x7 = false, PhoneNumber = "+91 6543210987", Email = "", Location = "Hostel Office", Description = "Hostel safety and resident support", DisplayOrder = 5 },
                    new EmergencyContact { Name = "Campus Counselor", Category = "Counseling", Department = "Counseling", Role = "Counselor", Availability = "Appointment / working hours", Is24x7 = false, PhoneNumber = "+91 5432109876", Email = "counseling@surakshasetu.edu", Location = "Counselling Center", Description = "Confidential emotional and wellbeing support", DisplayOrder = 6 },
                    new EmergencyContact { Name = "Anti-Ragging Committee", Category = "Grievance", Department = "Student Affairs", Role = "Committee", Availability = "University-configured", Is24x7 = false, PhoneNumber = "+91 1234567890", Email = "", Location = "Student Affairs", Description = "Ragging complaints and escalation", DisplayOrder = 7 },
                    new EmergencyContact { Name = "Grievance Officer", Category = "Grievance", Department = "Admin", Role = "Grievance Officer", Availability = "University-configured", Is24x7 = false, Email = "grievance@surakshasetu.edu", Location = "Grievance Cell", Description = "Formal grievance and complaint support", DisplayOrder = 8 },
                    new EmergencyContact { Name = "Cyber Safety / IT Support", Category = "Cyber Security", Department = "IT", Role = "IT Security", Availability = "Working hours", Is24x7 = false, PhoneNumber = "+91 4321098765", Email = "itsecurity@surakshasetu.edu", Location = "IT Help Desk", Description = "Cyber harassment, account and technology safety", DisplayOrder = 9 },
                    new EmergencyContact { Name = "Student Welfare Office", Category = "Student Welfare", Department = "Student Affairs", Role = "Welfare Officer", Availability = "Working hours", Is24x7 = false, PhoneNumber = "+91 2345678901", Email = "", Location = "Student Affairs", Description = "Student support, welfare and escalation", DisplayOrder = 10 },
                    new EmergencyContact { Name = "Transport Security", Category = "Transport", Department = "Transport", Role = "Transport Officer", Availability = "University-configured", Is24x7 = false, PhoneNumber = "+91 3210987654", Email = "", Location = "Transport Office", Description = "Safe campus transport and pickup-point concerns", DisplayOrder = 11 }
                );
                await context.SaveChangesAsync();
            }

            // 6. Seed Safety Reports
            if (context.SafetyReports.Count() < 13)
            {
                var securityUser = await userManager.FindByEmailAsync("security@surakshasetu.edu");
                var secUserId = securityUser?.Id ?? "test-sec-id";

                var student = await userManager.FindByEmailAsync("student@surakshasetu.edu");
                var studentId = student?.Id ?? "test-student-id";

                context.SafetyReports.AddRange(
                    new SafetyReport { UserId = studentId, ReportId = "SOS-2610-1001", IncidentLocation = "Main Library", Status = "Pending", ImmediateDanger = true, Severity = "Critical", IncidentCategory = "Emergency SOS", IncidentDate = DateTime.Today, IncidentTime = "14:30", Description = "Active SOS Triggered", CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
                    new SafetyReport { UserId = studentId, ReportId = "INC-2610-1002", IncidentLocation = "North Gate", Status = "In Progress", ImmediateDanger = false, Severity = "High", IncidentCategory = "Harassment", IncidentDate = DateTime.Today, IncidentTime = "10:00", Description = "Reported suspicious individual", CreatedAt = DateTime.UtcNow.AddHours(-2), AssignedTo = "security-new@surakshasetu.edu", UpdatedAt = DateTime.UtcNow.AddHours(-1) },
                    new SafetyReport { UserId = studentId, ReportId = "INC-2610-1003", IncidentLocation = "Girls Hostel A", Status = "Pending", ImmediateDanger = false, Severity = "Medium", IncidentCategory = "Infrastructure Issue", IncidentDate = DateTime.Today.AddDays(-1), IncidentTime = "20:00", Description = "Broken street light outside hostel", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                    new SafetyReport { UserId = studentId, ReportId = "INC-2610-1004", IncidentLocation = "Medical Center", Status = "Under Investigation", ImmediateDanger = false, Severity = "High", IncidentCategory = "Medical Emergency", IncidentDate = DateTime.Today, IncidentTime = "09:15", Description = "Student collapsed near building", CreatedAt = DateTime.UtcNow.AddHours(-4), AssignedTo = "security-new@surakshasetu.edu", UpdatedAt = DateTime.UtcNow.AddHours(-3) },
                    new SafetyReport { UserId = studentId, ReportId = "SOS-2610-1005", IncidentLocation = "Parking Lot", Status = "Assigned", ImmediateDanger = true, Severity = "Critical", IncidentCategory = "Emergency SOS", IncidentDate = DateTime.Today, IncidentTime = "11:45", Description = "Active SOS Triggered", CreatedAt = DateTime.UtcNow.AddHours(-1), AssignedTo = "security-new@surakshasetu.edu", UpdatedAt = DateTime.UtcNow.AddMinutes(-45) }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateTestUserAsync(UserManager<ApplicationUser> userManager, string email, string fullName, string role, string permissionClaim = null)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "SurakshaSetu@2026!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    
                    if (!string.IsNullOrEmpty(permissionClaim))
                    {
                        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Permission", permissionClaim));
                    }
                }
            }
        }
    }
}
