namespace SurakshaSetu.Models
{
    public enum IncidentSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum IncidentStatus
    {
        Submitted,
        UnderReview,
        Verified,
        Assigned,
        Investigation,
        Resolved,
        Rejected,
        Closed
    }
}
