namespace PulseGrid.Application.Services.Commands.DeleteMonitoredService;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseGrid.Application.Common.Interfaces;

public class DeleteMonitoredServiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteMonitoredServiceCommand>
{
    public async Task Handle(DeleteMonitoredServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await context.MonitoredServices
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service is null)
        {
            throw new KeyNotFoundException($"Monitored service with ID '{request.Id}' was not found.");
        }

        var logs = await context.HealthCheckLogs
            .Where(l => l.ServiceId == request.Id)
            .ToListAsync(cancellationToken);

        if (logs.Count > 0)
        {
            context.HealthCheckLogs.RemoveRange(logs);
        }

        var incidents = await context.Incidents
            .Include(i => i.Updates)
            .Where(i => i.ServiceId == request.Id)
            .ToListAsync(cancellationToken);

        if (incidents.Count > 0)
        {
            foreach (var incident in incidents)
            {
                context.IncidentUpdates.RemoveRange(incident.Updates);
            }

            context.Incidents.RemoveRange(incidents);
        }

        context.MonitoredServices.Remove(service);
        await context.SaveChangesAsync(cancellationToken);
    }
}
