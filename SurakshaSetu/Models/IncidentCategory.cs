namespace SurakshaSetu.Models
{
    public class IncidentCategory
    {
        public int IncidentCategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
