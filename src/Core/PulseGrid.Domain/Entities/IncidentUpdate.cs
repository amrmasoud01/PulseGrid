namespace PulseGrid.Domain.Entities;

using PulseGrid.Domain.Enums;

public class IncidentUpdate
{
    public Guid Id { get; set; }
    public Guid IncidentId { get; set; }
    public required string Message { get; set; }
    public IncidentStatus StatusSnapshot { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? AuthorUserId { get; set; }
}
