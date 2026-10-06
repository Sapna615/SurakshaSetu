using SurakshaSetu.Models;
using System.Collections.Generic;

namespace SurakshaSetu.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        
        // --- REAL DATABASE STATISTICS ---
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int InvestigationReports { get; set; }
        public int ResolvedReports { get; set; }

        // --- RECENT DATA ---
        // We pass a lightweight list of the most recent incidents
        public List<SafetyReport> RecentReports { get; set; } = new List<SafetyReport>();

        // --- NOTIFICATIONS ---
        public int UnreadNotificationsCount { get; set; }

        public int ActiveAlertsCount { get; set; }
        public List<SafetyAlert> ActiveAlerts { get; set; } = new List<SafetyAlert>();
        public List<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();
    }
}
