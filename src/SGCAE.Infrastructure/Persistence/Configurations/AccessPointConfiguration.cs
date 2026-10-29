using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCAE.Domain.Entities;

namespace SGCAE.Infrastructure.Persistence.Configurations;

public sealed class AccessPointConfiguration : IEntityTypeConfiguration<AccessPoint>
{
    public void Configure(EntityTypeBuilder<AccessPoint> builder)
    {
        builder.ToTable("AccessPoints");

        builder.HasKey(ap => ap.Id);

        builder.Property(ap => ap.Code)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(ap => ap.Code)
            .IsUnique();

        builder.Property(ap => ap.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ap => ap.Area)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ap => ap.Description)
            .HasMaxLength(250);

        builder.Property(ap => ap.IsRestrictedArea)
            .IsRequired();

        builder.Property(ap => ap.IsEnabled)
            .IsRequired();

        builder.Property(ap => ap.CreatedAtUtc)
            .IsRequired();

        builder.Ignore(ap => ap.DomainEvents);
    }
}

