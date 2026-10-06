using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SurakshaSetu.ViewModels
{
    public class AdminUserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string? Department { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; } // Derived from LockoutEnd
    }

    public class AdminUsersPageViewModel
    {
        public List<AdminUserListViewModel> Users { get; set; } = new List<AdminUserListViewModel>();
        
        // Filters
        public string? SearchQuery { get; set; }
        public string? RoleFilter { get; set; }
        public string? DepartmentFilter { get; set; }
        public string? StatusFilter { get; set; }
        
        public List<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableDepartments { get; set; } = new List<SelectListItem>();
    }

    public class AdminUserEditViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Student/Employee ID")]
        public string? StudentId { get; set; }
        
        public string? Department { get; set; }
        
        public int? Year { get; set; }
        
        [Display(Name = "Account Status")]
        public bool IsActive { get; set; }
        
        public List<string> CurrentRoles { get; set; } = new List<string>();
        
        [Display(Name = "Assign Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
        
        public List<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
    }
}
