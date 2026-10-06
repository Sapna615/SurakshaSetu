using System;
using System.ComponentModel.DataAnnotations;

namespace SurakshaSetu.ViewModels
{
    public class SafetyReportCreateViewModel
    {
        [Required(ErrorMessage = "Please select an incident category")]
        public string IncidentCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the incident location")]
        [StringLength(250)]
        public string IncidentLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select the incident date")]
        public DateTime IncidentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please enter the incident time")]
        public string IncidentTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a description")]
        [StringLength(5000, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 5000 characters")]
        public string Description { get; set; } = string.Empty;

        public bool IsAnonymous { get; set; }

        public string Severity { get; set; } = "Medium";

        public bool ImmediateDanger { get; set; }

        public bool MedicalAssistanceRequired { get; set; }

        public bool SecurityAssistanceRequired { get; set; }

        public string Source { get; set; } = "Direct Report";
    }
}
