using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SurakshaSetu.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? StudentId { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        public int? Year { get; set; }
        
    }
}
