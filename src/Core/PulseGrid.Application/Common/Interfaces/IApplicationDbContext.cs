namespace PulseGrid.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using PulseGrid.Domain.Entities;

public interface IApplicationDbContext
{
    DbSet<MonitoredService> MonitoredServices { get; }
    DbSet<Incident> Incidents { get; }
    DbSet<IncidentUpdate> IncidentUpdates { get; }
    DbSet<HealthCheckLog> HealthCheckLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
