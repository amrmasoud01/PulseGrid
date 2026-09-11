namespace PulseGrid.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseGrid.Domain.Entities;

public class MonitoredServiceConfiguration : IEntityTypeConfiguration<MonitoredService>
{
    public void Configure(EntityTypeBuilder<MonitoredService> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(s => s.TargetUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(s => s.IntervalSeconds)
            .IsRequired();

        builder.Property(s => s.TimeoutMs)
            .IsRequired();

        builder.Property(s => s.CurrentStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.LastCheckedAtUtc)
            .IsRequired(false);
    }
}
