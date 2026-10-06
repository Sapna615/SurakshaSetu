using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurakshaSetu.Models
{
    public class SafetyReportStatusHistory
    {
        [Key]
        public int Id { get; set; }

        public int SafetyReportId { get; set; }

        [MaxLength(50)]
        public string OldStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string NewStatus { get; set; } = string.Empty;

        [MaxLength(256)]
        public string ChangedBy { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        [ForeignKey("SafetyReportId")]
        public virtual SafetyReport? SafetyReport { get; set; }
    }
}
