namespace PulseGrid.Application.Common.Models;

using PulseGrid.Domain.Enums;

public record ServiceStatusDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string TargetUrl { get; init; }
    public required ServiceStatus Status { get; init; }
    public DateTime? LastCheckedAtUtc { get; init; }
    public long? RecentLatencyMs { get; init; }
}
