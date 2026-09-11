namespace PulseGrid.Application.Common.Models;

using PulseGrid.Domain.Enums;

public record SystemStatusOverviewDto
{
    public required ServiceStatus OverallStatus { get; init; }
    public required IReadOnlyList<ServiceStatusDto> Services { get; init; }
    public required IReadOnlyList<ActiveIncidentDto> ActiveIncidents { get; init; }
}
