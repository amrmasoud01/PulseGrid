namespace PulseGrid.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseGrid.Domain.Entities;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ServiceId)
            .IsRequired();

        builder.Property(i => i.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Severity)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.CreatedAtUtc)
            .IsRequired();

        builder.Property(i => i.ResolvedAtUtc)
            .IsRequired(false);

        builder.HasIndex(i => i.CreatedAtUtc);

        builder.HasOne<MonitoredService>()
            .WithMany()
            .HasForeignKey(i => i.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Updates)
            .WithOne()
            .HasForeignKey(u => u.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
