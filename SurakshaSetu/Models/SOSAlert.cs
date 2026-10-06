using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurakshaSetu.Models
{
    public class SOSAlert
    {
        [Key]
        [StringLength(50)]
        public string RequestId { get; set; } = $"SOS-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Created"; // Created, Acknowledged, Responding, Resolved, Cancelled

        [StringLength(250)]
        public string LocationDescription { get; set; }

        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
