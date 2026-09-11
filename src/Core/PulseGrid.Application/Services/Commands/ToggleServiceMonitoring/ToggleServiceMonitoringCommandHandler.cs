namespace PulseGrid.Application.Services.Commands.ToggleServiceMonitoring;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseGrid.Application.Common.Interfaces;

public class ToggleServiceMonitoringCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleServiceMonitoringCommand, bool>
{
    public async Task<bool> Handle(ToggleServiceMonitoringCommand request, CancellationToken cancellationToken)
    {
        var service = await context.MonitoredServices
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service is null)
        {
            throw new KeyNotFoundException($"Monitored service with ID '{request.Id}' was not found.");
        }

        service.IsActive = !service.IsActive;
        await context.SaveChangesAsync(cancellationToken);

        return service.IsActive;
    }
}
