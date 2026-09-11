namespace PulseGrid.Application.SystemStatus.Queries.GetSystemStatus;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseGrid.Application.Common.Interfaces;
using PulseGrid.Application.Common.Models;
using PulseGrid.Domain.Entities;
using PulseGrid.Domain.Enums;

public class GetSystemStatusQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSystemStatusQuery, SystemStatusOverviewDto>
{
    public async Task<SystemStatusOverviewDto> Handle(GetSystemStatusQuery request, CancellationToken cancellationToken)
    {
        var services = await context.MonitoredServices
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var activeIncidents = await context.Incidents
            .AsNoTracking()
            .Where(i => i.Status != IncidentStatus.Resolved)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var serviceNames = services.ToDictionary(s => s.Id, s => s.Name);

        var latestLatencies = await context.HealthCheckLogs
            .AsNoTracking()
            .GroupBy(l => l.ServiceId)
            .Select(g => new
            {
                ServiceId = g.Key,
                RecentLatencyMs = g.OrderByDescending(x => x.CheckedAtUtc).Select(x => (long?)x.LatencyMs).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.ServiceId, x => x.RecentLatencyMs, cancellationToken);

        var serviceDtos = services.Select(s => new ServiceStatusDto
        {
            Id = s.Id,
            Name = s.Name,
            TargetUrl = s.TargetUrl,
            Status = s.CurrentStatus,
            LastCheckedAtUtc = s.LastCheckedAtUtc,
            RecentLatencyMs = latestLatencies.GetValueOrDefault(s.Id),
            IsActive = s.IsActive
        }).ToList();

        var incidentDtos = activeIncidents.Select(i => new ActiveIncidentDto
        {
            Id = i.Id,
            ServiceId = i.ServiceId,
            ServiceName = serviceNames.GetValueOrDefault(i.ServiceId, "Unknown Service"),
            Title = i.Title,
            Severity = i.Severity,
            Status = i.Status,
            CreatedAtUtc = i.CreatedAtUtc
        }).ToList();

        var overallStatus = DetermineOverallStatus(services, activeIncidents);

        return new SystemStatusOverviewDto
        {
            OverallStatus = overallStatus,
            Services = serviceDtos,
            ActiveIncidents = incidentDtos
        };
    }

    private static ServiceStatus DetermineOverallStatus(
        IReadOnlyList<MonitoredService> services,
        IReadOnlyList<Incident> incidents)
    {
        var activeServices = services.Where(s => s.IsActive).ToList();

        if (activeServices.Any(s => s.CurrentStatus == ServiceStatus.Down) ||
            incidents.Any(i => i.Severity == IncidentSeverity.Critical))
        {
            return ServiceStatus.Down;
        }

        if (activeServices.Any(s => s.CurrentStatus == ServiceStatus.Degraded) ||
            incidents.Count > 0)
        {
            return ServiceStatus.Degraded;
        }

        return ServiceStatus.Operational;
    }
}
