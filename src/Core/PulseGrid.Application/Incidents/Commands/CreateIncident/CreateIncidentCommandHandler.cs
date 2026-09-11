namespace PulseGrid.Application.Incidents.Commands.CreateIncident;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseGrid.Application.Common.Interfaces;
using PulseGrid.Domain.Entities;
using PulseGrid.Domain.Enums;

public class CreateIncidentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateIncidentCommand, Guid>
{
    public async Task<Guid> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        var serviceExists = await context.MonitoredServices
            .AnyAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (!serviceExists)
        {
            throw new KeyNotFoundException($"Monitored service with ID '{request.ServiceId}' was not found.");
        }

        var incidentId = Guid.NewGuid();
        var timestampUtc = DateTime.UtcNow;

        var incident = new Incident
        {
            Id = incidentId,
            ServiceId = request.ServiceId,
            Title = request.Title,
            Severity = request.Severity,
            Status = IncidentStatus.Investigating,
            CreatedAtUtc = timestampUtc,
            ResolvedAtUtc = null
        };

        if (!string.IsNullOrWhiteSpace(request.InitialMessage))
        {
            incident.Updates.Add(new IncidentUpdate
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                Message = request.InitialMessage,
                StatusSnapshot = IncidentStatus.Investigating,
                TimestampUtc = timestampUtc,
                AuthorUserId = null
            });
        }

        context.Incidents.Add(incident);
        await context.SaveChangesAsync(cancellationToken);

        return incident.Id;
    }
}
