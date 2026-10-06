using System;
using System.Collections.Generic;
using SurakshaSetu.Models;

namespace SurakshaSetu.ViewModels
{
    public class SecurityDashboardViewModel
    {
        public string OfficerName { get; set; } = string.Empty;
        public int NewReports { get; set; }
        public int ActiveSOS { get; set; }
        public int InProgress { get; set; }
        
        // Using SafetyReport instead of Incident since migrations are not allowed right now
        // and SafetyReport is already used by Student Dashboard
        public List<SafetyReport> ActiveSOSList { get; set; } = new List<SafetyReport>();
        public List<SafetyReport> IncidentQueue { get; set; } = new List<SafetyReport>();
    }

    public class AdminDashboardViewModel
    {
        public string AdminName { get; set; } = string.Empty;
        
        // System Overview metrics
        public int TotalStudents { get; set; }
        public int TotalCounselors { get; set; }
        public int TotalSecurityStaff { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TotalReports { get; set; }
        public int OpenCases { get; set; }
        public int ResolvedCases { get; set; }
        public int ActiveSOS { get; set; }
        public int ActiveSafetyAlerts { get; set; }
        
        // Lists for sections
        public List<SafetyReport> RecentReports { get; set; } = new List<SafetyReport>();
        public List<SOSAlert> RecentSOSRequests { get; set; } = new List<SOSAlert>();
        public List<SafetyAlert> RecentSafetyAlerts { get; set; } = new List<SafetyAlert>();
        public List<ApplicationUser> RecentUserRegistrations { get; set; } = new List<ApplicationUser>();
    }

    public class SuperAdminDashboardViewModel
    {
        public string SuperAdminName { get; set; } = string.Empty;
        
        public int TotalStudents { get; set; }
        public int TotalSecurityOfficers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalSuperAdmins { get; set; }
        
        // A placeholder for audit logs
        public List<string> RecentAuditLogs { get; set; } = new List<string>();
    }
}
