using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SurakshaSetu.Models;

namespace SurakshaSetu.ViewModels
{
    public class AdminSafetyAlertsPageViewModel
    {
        public List<SafetyAlert> Alerts { get; set; } = new List<SafetyAlert>();
        
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
        public string? CategoryFilter { get; set; }
        
        public List<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();
    }

    public class AdminSafetyAlertFormViewModel
    {
        public int SafetyAlertId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Low";
        
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;
        
        [StringLength(2000)]
        [Display(Name = "Recommended Action")]
        public string RecommendedAction { get; set; } = string.Empty;
        
        [Display(Name = "Expiration Date")]
        public DateTime? ExpiresAt { get; set; }
        
        [Display(Name = "Status")]
        public bool IsActive { get; set; } = true;
        
        public List<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Weather", Text = "Weather" },
            new SelectListItem { Value = "Security", Text = "Security" },
            new SelectListItem { Value = "Infrastructure", Text = "Infrastructure" },
            new SelectListItem { Value = "Health", Text = "Health" },
            new SelectListItem { Value = "General", Text = "General" }
        };
        
        public List<SelectListItem> AvailablePriorities { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Low", Text = "Low" },
            new SelectListItem { Value = "Medium", Text = "Medium" },
            new SelectListItem { Value = "High", Text = "High" },
            new SelectListItem { Value = "Critical", Text = "Critical" }
        };
    }
}
