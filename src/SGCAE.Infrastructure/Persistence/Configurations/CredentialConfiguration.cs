using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCAE.Domain.Entities;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Infrastructure.Persistence.Configurations;

public sealed class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.ToTable("Credentials");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .HasConversion(
                code => code.Value,
                value => CredentialCode.Create(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.IssuedAtUtc)
            .IsRequired();

        builder.Property(c => c.BlockReason)
            .HasMaxLength(250);

        builder.Property(c => c.RevocationReason)
            .HasMaxLength(250);

        builder.Ignore(c => c.DomainEvents);
    }
}

