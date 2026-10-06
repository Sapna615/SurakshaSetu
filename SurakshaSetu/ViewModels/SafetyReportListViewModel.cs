using System;
using System.Collections.Generic;
using SurakshaSetu.Models;

namespace SurakshaSetu.ViewModels
{
    public class SafetyReportListViewModel
    {
        public List<SafetyReport> Reports { get; set; } = new List<SafetyReport>();
        
        public int TotalReports { get; set; }
        public int PendingCount { get; set; }
        public int UnderReviewCount { get; set; }
        public int InvestigationCount { get; set; }
        public int ResolvedCount { get; set; }
        public int RejectedCount { get; set; }
        public int ActionTakenCount { get; set; }

        public SafetyReportFilterViewModel Filter { get; set; } = new SafetyReportFilterViewModel();

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
    }

    public class SafetyReportFilterViewModel
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? Category { get; set; }
        public string? Severity { get; set; }
        public string? DateRange { get; set; }
        public string? SortBy { get; set; }
    }
}
