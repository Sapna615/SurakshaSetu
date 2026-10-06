using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SurakshaSetu.Models;

namespace SurakshaSetu.ViewModels
{
    public class SecurityIncidentQueueViewModel
    {
        public List<SafetyReport> Incidents { get; set; } = new List<SafetyReport>();
        
        public string? SearchQuery { get; set; }
        public string? Category { get; set; }
        public string? Severity { get; set; }
        public string? Status { get; set; }
        public string? DateRange { get; set; }
        public string? SortBy { get; set; }
        
        public List<string> AvailableCategories { get; set; } = new List<string>();
    }

    public class SecurityIncidentDetailsViewModel
    {
        public SafetyReport Incident { get; set; }
        public ApplicationUser? Reporter { get; set; }
        public ApplicationUser? AssignedOfficer { get; set; }
        public string LocationName { get; set; } = string.Empty;
        
        // Form properties for updates
        public string NewStatus { get; set; } = string.Empty;
        public string ResponseNote { get; set; } = string.Empty;
        public string AssignToEmail { get; set; } = string.Empty;
        
        public List<ApplicationUser> AvailableOfficers { get; set; } = new List<ApplicationUser>();
    }

    public class SecurityProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
