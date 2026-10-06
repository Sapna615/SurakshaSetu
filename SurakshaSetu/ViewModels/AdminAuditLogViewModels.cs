using System;
using System.Collections.Generic;
using SurakshaSetu.Models;

namespace SurakshaSetu.ViewModels
{
    public class AdminAuditLogsPageViewModel
    {
        public List<AuditLog> Logs { get; set; } = new List<AuditLog>();
        
        public string? SearchQuery { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
