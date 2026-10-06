using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurakshaSetu.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [MaxLength(100)]
        public string? EntityId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(2000)]
        public string? Details { get; set; }
    }
}
