namespace PulseGrid.Application.Common.Models;

using PulseGrid.Domain.Enums;

public record ActiveIncidentDto
{
    public required Guid Id { get; init; }
    public required Guid ServiceId { get; init; }
    public required string ServiceName { get; init; }
    public required string Title { get; init; }
    public required IncidentSeverity Severity { get; init; }
    public required IncidentStatus Status { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
