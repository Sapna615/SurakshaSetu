using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SurakshaSetu.Models;

namespace SurakshaSetu.ViewModels
{
    public class AdminLocationsPageViewModel
    {
        public List<CampusLocation> Locations { get; set; } = new List<CampusLocation>();
        
        public string? SearchQuery { get; set; }
        public string? TypeFilter { get; set; }
        
        public List<SelectListItem> AvailableTypes { get; set; } = new List<SelectListItem>();
    }

    public class AdminLocationFormViewModel
    {
        public int CampusLocationId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
        public double? Longitude { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Location Type")]
        public string LocationType { get; set; } = "CampusBuilding";

        [Display(Name = "Status (Active)")]
        public bool IsActive { get; set; } = true;

        public List<SelectListItem> AvailableTypes { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "CampusBuilding", Text = "Campus Building" },
            new SelectListItem { Value = "ParkingLot", Text = "Parking Lot" },
            new SelectListItem { Value = "Hostel", Text = "Hostel / Dormitory" },
            new SelectListItem { Value = "SportsFacility", Text = "Sports Facility" },
            new SelectListItem { Value = "EmergencyPoint", Text = "Emergency Point (SOS)" },
            new SelectListItem { Value = "MedicalCenter", Text = "Medical Center" },
            new SelectListItem { Value = "Gate", Text = "Entrance / Gate" },
            new SelectListItem { Value = "Cafeteria", Text = "Cafeteria / Food Court" }
        };
    }
}
