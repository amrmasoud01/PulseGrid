namespace PulseGrid.Application.Common.Interfaces;

using PulseGrid.Application.Common.Models;
using PulseGrid.Domain.Enums;

public interface IStatusNotifier
{
    Task BroadcastStatusUpdateAsync(SystemStatusOverviewDto overview, CancellationToken ct = default);
    Task BroadcastServicePingAsync(Guid serviceId, ServiceStatus status, long latencyMs, CancellationToken ct = default);
}
