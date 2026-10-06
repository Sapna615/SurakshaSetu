using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SurakshaSetu.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SurakshaSetu.ViewModels
{
    public class CreateIncidentViewModel
    {
        [Required(ErrorMessage = "Please select an incident category.")]
        [Display(Name = "Incident Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please select a campus location.")]
        [Display(Name = "Campus Location")]
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Please provide a description of what happened.")]
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide the date of the incident.")]
        [Display(Name = "Date of Incident")]
        [DataType(DataType.Date)]
        public DateTime IncidentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please provide the time of the incident.")]
        [Display(Name = "Time of Incident")]
        [DataType(DataType.Time)]
        public TimeSpan IncidentTime { get; set; } = DateTime.Now.TimeOfDay;

        [Required]
        public IncidentSeverity Severity { get; set; } = IncidentSeverity.Low;

        [Display(Name = "Report Anonymously")]
        public bool IsAnonymous { get; set; }

        [Display(Name = "Attach Supporting Evidence")]
        public IFormFile? EvidenceFile { get; set; }

        // These properties are just for rendering the dropdown lists in the UI
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Locations { get; set; } = new List<SelectListItem>();
    }
}
