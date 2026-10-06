using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SurakshaSetu.Models;

namespace SurakshaSetu.ViewModels
{
    public class AdminEmergencyContactsPageViewModel
    {
        public List<EmergencyContact> Contacts { get; set; } = new List<EmergencyContact>();
        
        public string? SearchQuery { get; set; }
        public string? CategoryFilter { get; set; }
        
        public List<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();
    }

    public class AdminEmergencyContactFormViewModel
    {
        public int EmergencyContactId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string Department { get; set; } = string.Empty;

        [StringLength(150)]
        public string Role { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string Category { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string Availability { get; set; } = string.Empty;

        [Display(Name = "Is 24x7 Available")]
        public bool Is24x7 { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [StringLength(500)]
        [Display(Name = "Website URL")]
        public string WebsiteUrl { get; set; } = string.Empty;

        [Display(Name = "Can Call Directly")]
        public bool CanCall { get; set; } = true;

        [Display(Name = "Is Emergency Contact")]
        public bool IsEmergency { get; set; }

        [Display(Name = "Show In Quick Guide")]
        public bool ShowInQuickGuide { get; set; } = true;

        [Display(Name = "Status (Active)")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        public List<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Emergency", Text = "Emergency" },
            new SelectListItem { Value = "Security", Text = "Security" },
            new SelectListItem { Value = "Medical", Text = "Medical" },
            new SelectListItem { Value = "Counseling", Text = "Counseling" },
            new SelectListItem { Value = "Administration", Text = "Administration" },
            new SelectListItem { Value = "Maintenance", Text = "Maintenance" }
        };
    }
}
