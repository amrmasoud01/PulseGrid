namespace PulseGrid.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseGrid.Domain.Entities;

public class IncidentUpdateConfiguration : IEntityTypeConfiguration<IncidentUpdate>
{
    public void Configure(EntityTypeBuilder<IncidentUpdate> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.IncidentId)
            .IsRequired();

        builder.Property(u => u.Message)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(u => u.StatusSnapshot)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.TimestampUtc)
            .IsRequired();

        builder.Property(u => u.AuthorUserId)
            .HasMaxLength(100)
            .IsRequired(false);
    }
}
