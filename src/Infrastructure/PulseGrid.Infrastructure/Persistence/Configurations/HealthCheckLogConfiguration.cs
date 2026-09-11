namespace PulseGrid.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseGrid.Domain.Entities;

public class HealthCheckLogConfiguration : IEntityTypeConfiguration<HealthCheckLog>
{
    public void Configure(EntityTypeBuilder<HealthCheckLog> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.ServiceId)
            .IsRequired();

        builder.Property(h => h.StatusCode)
            .IsRequired(false);

        builder.Property(h => h.LatencyMs)
            .IsRequired();

        builder.Property(h => h.IsSuccess)
            .IsRequired();

        builder.Property(h => h.CheckedAtUtc)
            .IsRequired();

        builder.HasIndex(h => new { h.ServiceId, h.CheckedAtUtc });

        builder.HasIndex(h => h.CheckedAtUtc);

        builder.HasOne<MonitoredService>()
            .WithMany()
            .HasForeignKey(h => h.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
