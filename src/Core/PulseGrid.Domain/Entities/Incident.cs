namespace PulseGrid.Domain.Entities;

using PulseGrid.Domain.Enums;

public class Incident
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public required string Title { get; set; }
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public ICollection<IncidentUpdate> Updates { get; set; } = [];
}
