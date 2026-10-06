using System.Collections.Generic;
using SurakshaSetu.Models;

namespace SurakshaSetu.ViewModels
{
    public class SafetyReportDetailsViewModel
    {
        public SafetyReport Report { get; set; } = new SafetyReport();
        public List<SafetyReportStatusHistory> StatusHistory { get; set; } = new List<SafetyReportStatusHistory>();
    }
}
