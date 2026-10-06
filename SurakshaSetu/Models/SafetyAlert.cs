using System;
using System.ComponentModel.DataAnnotations;

namespace SurakshaSetu.Models
{
    public class SafetyAlert
    {
        [Key]
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

        [StringLength(150)]
        public string PublishedBy { get; set; } = string.Empty;

        [StringLength(2000)]
        public string RecommendedAction { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime? ExpiresAt { get; set; }
    }
}
