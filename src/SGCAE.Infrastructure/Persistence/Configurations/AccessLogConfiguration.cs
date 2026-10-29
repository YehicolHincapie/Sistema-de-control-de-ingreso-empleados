using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCAE.Domain.Entities;

namespace SGCAE.Infrastructure.Persistence.Configurations;

public sealed class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        builder.ToTable("AccessLogs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.CredentialCodeUsed)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(al => al.MovementType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(al => al.ValidationResult)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(al => al.RejectionReason)
            .HasMaxLength(250);

        builder.Property(al => al.TimestampUtc)
            .IsRequired();

        builder.HasIndex(al => al.TimestampUtc);
        builder.HasIndex(al => al.EmployeeId);
        builder.HasIndex(al => al.AccessPointId);
        builder.HasIndex(al => al.ValidationResult);

        builder.HasOne(al => al.Employee)
            .WithMany()
            .HasForeignKey(al => al.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(al => al.Credential)
            .WithMany()
            .HasForeignKey(al => al.CredentialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(al => al.AccessPoint)
            .WithMany()
            .HasForeignKey(al => al.AccessPointId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(al => al.DomainEvents);
    }
}

