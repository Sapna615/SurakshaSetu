using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using SurakshaSetu.Models;

namespace SurakshaSetu.ViewModels
{
    public class AdminCasesPageViewModel
    {
        public List<SafetyReport> Cases { get; set; } = new List<SafetyReport>();
        
        // Filters
        public string? SearchQuery { get; set; }
        public string? CategoryFilter { get; set; }
        public string? SeverityFilter { get; set; }
        public string? StatusFilter { get; set; }
        public string? AssignedFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public List<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableStatuses { get; set; } = new List<SelectListItem>();
    }

    public class AdminCaseDetailViewModel
    {
        public SafetyReport Report { get; set; } = new SafetyReport();
        public List<SafetyReportStatusHistory> Timeline { get; set; } = new List<SafetyReportStatusHistory>();
        
        // Reassignment
        public string? NewAssignee { get; set; }
        public List<SelectListItem> SecurityStaff { get; set; } = new List<SelectListItem>();
    }
}
