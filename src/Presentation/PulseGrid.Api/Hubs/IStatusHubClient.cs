namespace PulseGrid.Api.Hubs;

using PulseGrid.Application.Common.Models;
using PulseGrid.Domain.Enums;

public interface IStatusHubClient
{
    Task ReceiveStatusUpdate(SystemStatusOverviewDto overview);
    Task ReceiveServicePing(Guid serviceId, ServiceStatus status, long latencyMs);
}
