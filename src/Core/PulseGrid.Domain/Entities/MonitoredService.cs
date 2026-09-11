namespace PulseGrid.Domain.Entities;

using PulseGrid.Domain.Enums;

public class MonitoredService
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string TargetUrl { get; set; }
    public int IntervalSeconds { get; set; }
    public int TimeoutMs { get; set; }
    public ServiceStatus CurrentStatus { get; set; }
    public DateTime? LastCheckedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
