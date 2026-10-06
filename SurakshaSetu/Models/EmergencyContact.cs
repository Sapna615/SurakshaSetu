// using System;
// using System.ComponentModel.DataAnnotations;

// namespace SurakshaSetu.Models
// {
//     public class EmergencyContact
//     {
//         [Key]
//         public int EmergencyContactId { get; set; }

//         [Required]
//         [StringLength(100)]
//         public string Name { get; set; }

//         [StringLength(100)]
//         public string Department { get; set; }

//         [StringLength(100)]
//         public string Role { get; set; }

//         [Required]
//         [StringLength(50)]
//         public string Category { get; set; }

//         [StringLength(50)]
//         public string PhoneNumber { get; set; }

//         [StringLength(100)]
//         public string Email { get; set; }

//         [StringLength(200)]
//         public string Location { get; set; }

//         [StringLength(500)]
//         public string Description { get; set; }

//         [StringLength(100)]
//         public string Availability { get; set; }

//         public bool Is24x7 { get; set; }
        
//         public double? Latitude { get; set; }
        
//         public double? Longitude { get; set; }

//         public bool IsActive { get; set; } = true;
        
//         public int DisplayOrder { get; set; } = 0;
        
//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//     }
// }



using System;
using System.ComponentModel.DataAnnotations;

namespace SurakshaSetu.Models
{
    public class EmergencyContact
    {
        [Key]
        public int EmergencyContactId { get; set; }

        // Basic information
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string Department { get; set; } = string.Empty;

        [StringLength(150)]
        public string Role { get; set; } = string.Empty;

        // Emergency / Support / Medical / Security etc.
        [Required]
        [StringLength(80)]
        public string Category { get; set; } = string.Empty;

        // Contact information
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        // Description shown to students
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        // Example:
        // 24×7
        // Working Hours
        // Appointment Required
        [StringLength(100)]
        public string Availability { get; set; } = string.Empty;

        public bool Is24x7 { get; set; }

        // Map coordinates
        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // Optional website / information URL
        [StringLength(500)]
        public string WebsiteUrl { get; set; } = string.Empty;

        // Whether student can directly call this contact
        public bool CanCall { get; set; } = true;

        // Whether this contact should appear in emergency section
        public bool IsEmergency { get; set; }

        // Whether it should appear in "Who should I contact?"
        public bool ShowInQuickGuide { get; set; } = true;

        // Active/inactive
        public bool IsActive { get; set; } = true;

        // Display ordering
        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
