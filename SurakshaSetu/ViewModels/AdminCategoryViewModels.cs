using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SurakshaSetu.Models;

namespace SurakshaSetu.ViewModels
{
    public class AdminCategoriesPageViewModel
    {
        public List<IncidentCategory> Categories { get; set; } = new List<IncidentCategory>();
        
        public string? SearchQuery { get; set; }
    }

    public class AdminCategoryFormViewModel
    {
        public int IncidentCategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Status (Active)")]
        public bool IsActive { get; set; } = true;
    }
}
