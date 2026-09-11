namespace PulseGrid.BackgroundJobs.Services;

using Microsoft.Extensions.Logging;
using PulseGrid.Application.Common.Interfaces;
using PulseGrid.Application.Common.Models;
using PulseGrid.Domain.Enums;

public class LoggingStatusNotifier(ILogger<LoggingStatusNotifier> logger) : IStatusNotifier
{
    public Task BroadcastStatusUpdateAsync(SystemStatusOverviewDto overview, CancellationToken ct = default)
    {
        logger.LogInformation("Status update broadcast: OverallStatus={OverallStatus}", overview.OverallStatus);
        return Task.CompletedTask;
    }

    public Task BroadcastServicePingAsync(Guid serviceId, ServiceStatus status, long latencyMs, CancellationToken ct = default)
    {
        logger.LogDebug("Service ping broadcast: ServiceId={ServiceId}, Status={Status}, Latency={LatencyMs}ms",
            serviceId, status, latencyMs);
        return Task.CompletedTask;
    }
}
