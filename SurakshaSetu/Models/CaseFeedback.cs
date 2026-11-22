using System;
using System.ComponentModel.DataAnnotations;

namespace SurakshaSetu.Models
{
    public class CaseFeedback
    {
        public int CaseFeedbackId { get; set; }

        [Required]
        public int SafetyReportId { get; set; }
        public virtual SafetyReport? SafetyReport { get; set; }

        [Required]
        [MaxLength(30)]
        public string ReportId { get; set; } = string.Empty;

        // User who submitted the feedback
        public string? UserId { get; set; }
        public string? SubmitterName { get; set; }

        // 1 to 5 stars
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public bool WasResolvedSatisfactorily { get; set; } = true;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
