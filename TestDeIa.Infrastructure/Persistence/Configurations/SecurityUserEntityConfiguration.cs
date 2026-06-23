using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SecurityUserEntityConfiguration : IEntityTypeConfiguration<SecurityUserEntity>
{
    public void Configure(EntityTypeBuilder<SecurityUserEntity> builder)
    {
        builder.ToTable("SecurityUsers");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.UserName)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(user => user.NormalizedUserName)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(user => user.DisplayName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(user => user.NormalizedEmail)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(user => user.NormalizedUserName)
            .IsUnique();

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique();

        builder.HasData(new SecurityUserEntity
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            PersonaId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            DisplayName = "Administrador",
            Email = "admin@testdeia.local",
            NormalizedEmail = "ADMIN@TESTDEIA.LOCAL",
            PasswordHash = "0A5BC3E342432F1BAD92FFD51B785343EC72906CDBA6A26131060B008E786656",
            IsActive = true,
            CreatedAt = new DateTimeOffset(2026, 6, 17, 0, 0, 0, TimeSpan.Zero)
        });

        builder.HasOne(user => user.Persona)
            .WithMany(persona => persona.SecurityUsers)
            .HasForeignKey(user => user.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
