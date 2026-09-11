namespace PulseGrid.Api.Services;

using Microsoft.AspNetCore.SignalR;
using PulseGrid.Api.Hubs;
using PulseGrid.Application.Common.Interfaces;
using PulseGrid.Application.Common.Models;
using PulseGrid.Domain.Enums;

public class StatusNotifier(IHubContext<StatusHub, IStatusHubClient> hubContext) : IStatusNotifier
{
    public async Task BroadcastStatusUpdateAsync(SystemStatusOverviewDto overview, CancellationToken ct = default)
    {
        await hubContext.Clients.All.ReceiveStatusUpdate(overview);
    }

    public async Task BroadcastServicePingAsync(Guid serviceId, ServiceStatus status, long latencyMs, CancellationToken ct = default)
    {
        await hubContext.Clients.All.ReceiveServicePing(serviceId, status, latencyMs);
    }
}
