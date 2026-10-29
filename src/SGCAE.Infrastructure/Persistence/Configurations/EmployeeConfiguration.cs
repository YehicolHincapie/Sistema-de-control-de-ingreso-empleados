using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCAE.Domain.Entities;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.IdentificationNumber)
            .HasConversion(
                id => id.Value,
                value => IdentificationNumber.Create(value))
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(e => e.IdentificationNumber)
            .IsUnique();

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Create(value))
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.Position)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Department)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.FullName);

        builder.HasMany(e => e.Credentials)
            .WithOne(c => c.Employee)
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

