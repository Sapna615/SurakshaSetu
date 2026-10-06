using System.Collections.Generic;

namespace SurakshaSetu.ViewModels
{
    public class SafetyMapDataViewModel
    {
        public List<MapLocationDto> Locations { get; set; } = new List<MapLocationDto>();
    }

    public class MapLocationDto
    {
        public int LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationType { get; set; } = string.Empty;
        
        // Incident counts to show aggregated risk without leaking details
        public int RecentIncidentsCount { get; set; }
        public int InfrastructureIssuesCount { get; set; }
        
        // Active safety alerts at this location
        public int ActiveAlertsCount { get; set; }
    }
}
