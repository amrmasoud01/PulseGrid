namespace PulseGrid.Infrastructure.Persistence;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PulseGrid.Application.Common.Interfaces;
using PulseGrid.Domain.Entities;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<MonitoredService> MonitoredServices => Set<MonitoredService>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentUpdate> IncidentUpdates => Set<IncidentUpdate>();
    public DbSet<HealthCheckLog> HealthCheckLogs => Set<HealthCheckLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
