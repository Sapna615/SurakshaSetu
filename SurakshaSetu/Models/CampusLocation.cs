namespace SurakshaSetu.Models
{
    public class CampusLocation
    {
        public int CampusLocationId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // Added for Map Categorization
        public string LocationType { get; set; } = "CampusBuilding";

        public bool IsActive { get; set; } = true;

    }
}
