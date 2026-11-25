using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;

namespace SurakshaSetu
{
    class DbCheck
    {
        public static void Run(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=SurakshaSetuDb;User Id=sa;Password=SurakshaSetu@2026!;TrustServerCertificate=True;");
            
            using var context = new ApplicationDbContext(optionsBuilder.Options);

            Console.WriteLine("==========================================================================================");
            Console.WriteLine("                       🗄️  SURAKSHA SETU DATABASE INSPECTOR                              ");
            Console.WriteLine("==========================================================================================");

            // 1. Registered Users & Roles
            var users = context.Users.AsNoTracking().ToList();
            var roles = context.Roles.AsNoTracking().ToList();
            var userRoles = context.UserRoles.AsNoTracking().ToList();

            Console.WriteLine($"\n[👥 AspNetUsers - Registered Accounts ({users.Count})]");
            Console.WriteLine(string.Format("{0,-32} | {0,-26} | {1,-16} | {2,-12}", "Full Name", "Email", "Role", "Phone"));
            Console.WriteLine(new string('-', 90));
            foreach (var u in users)
            {
                var roleId = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.RoleId;
                var roleName = roles.FirstOrDefault(r => r.Id == roleId)?.Name ?? "User";
                Console.WriteLine(string.Format("{0,-32} | {1,-26} | {2,-16} | {3,-12}", 
                    (u.FullName ?? "—").PadRight(32).Substring(0, 32), 
                    u.Email, 
                    roleName, 
                    u.PhoneNumber ?? "—"));
            }

            // 2. Incident & SOS Reports
            var reports = context.SafetyReports.AsNoTracking().OrderByDescending(r => r.CreatedAt).Take(10).ToList();
            Console.WriteLine($"\n[🚨 SafetyReports - Recent Incidents & SOS ({context.SafetyReports.Count()} total)]");
            Console.WriteLine(string.Format("{0,-16} | {1,-20} | {2,-10} | {3,-12} | {4,-20}", "Report ID", "Category", "Severity", "Status", "Location"));
            Console.WriteLine(new string('-', 90));
            foreach (var r in reports)
            {
                Console.WriteLine(string.Format("{0,-16} | {1,-20} | {2,-10} | {3,-12} | {4,-20}",
                    r.ReportId,
                    r.IncidentCategory,
                    r.Severity,
                    r.Status,
                    (r.IncidentLocation ?? "—").PadRight(20).Substring(0, 20)));
            }

            // 3. Student Feedbacks
            var feedbacks = context.CaseFeedbacks.AsNoTracking().OrderByDescending(f => f.SubmittedAt).Take(5).ToList();
            Console.WriteLine($"\n[⭐ CaseFeedbacks - Student Ratings ({context.CaseFeedbacks.Count()} total)]");
            Console.WriteLine(string.Format("{0,-16} | {1,-8} | {2,-12} | {3,-40}", "Report ID", "Rating", "Satisfied", "Comments"));
            Console.WriteLine(new string('-', 90));
            if (!feedbacks.Any())
            {
                Console.WriteLine("No student feedbacks submitted yet.");
            }
            else
            {
                foreach (var f in feedbacks)
                {
                    var comment = (f.Comments ?? "—").Replace("\n", " ");
                    if (comment.Length > 38) comment = comment.Substring(0, 35) + "...";
                    Console.WriteLine(string.Format("{0,-16} | {1,-8} | {2,-12} | {3,-40}",
                        f.ReportId,
                        $"{f.Rating}/5 ★",
                        f.WasResolvedSatisfactorily ? "Yes" : "No",
                        comment));
                }
            }

            Console.WriteLine("\n==========================================================================================\n");
        }
    }
}
