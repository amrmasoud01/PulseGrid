namespace PulseGrid.Domain.Entities;

public class HealthCheckLog
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public int? StatusCode { get; set; }
    public long LatencyMs { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CheckedAtUtc { get; set; }
}
