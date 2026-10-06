using System;
using System.ComponentModel.DataAnnotations;

namespace SurakshaSetu.Models
{
    public class SafetyReport
    {
        public int SafetyReportId { get; set; }

        [Required]
        [MaxLength(30)]
        public string ReportId { get; set; } = string.Empty;

        // Null when report is anonymous
        public string? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string IncidentCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string IncidentLocation { get; set; } = string.Empty;

        [Required]
        public DateTime IncidentDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string IncidentTime { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string Description { get; set; } = string.Empty;

        public bool IsAnonymous { get; set; }

        [MaxLength(20)]
        public string Severity { get; set; } = "Medium";

        public bool ImmediateDanger { get; set; }
        
        public bool MedicalAssistanceRequired { get; set; }
        
        public bool SecurityAssistanceRequired { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [MaxLength(256)]
        public string? AssignedTo { get; set; }

        [MaxLength(2000)]
        public string? ResolutionNotes { get; set; }

        [MaxLength(100)]
        public string Source { get; set; } = "Direct Report";

        public virtual ICollection<SafetyReportStatusHistory> StatusHistory { get; set; } = new List<SafetyReportStatusHistory>();
    }
}
