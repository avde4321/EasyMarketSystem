using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SecurityUserPuntoEmisionEntityConfiguration : IEntityTypeConfiguration<SecurityUserPuntoEmisionEntity>
{
    public void Configure(EntityTypeBuilder<SecurityUserPuntoEmisionEntity> builder)
    {
        builder.ToTable("SecurityUserPuntosEmision");

        builder.HasKey(current => new { current.SecurityUserId, current.EmpresaPuntoEmisionId });

        builder.Property(current => current.CreatedAt)
            .IsRequired();

        builder.HasIndex(current => new { current.EmpresaId, current.SecurityUserId });
        builder.HasIndex(current => new { current.EmpresaId, current.EmpresaPuntoEmisionId });

        builder.HasOne<SecurityUserEntity>()
            .WithMany()
            .HasForeignKey(current => current.SecurityUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<EmpresaPuntoEmisionEntity>()
            .WithMany()
            .HasForeignKey(current => current.EmpresaPuntoEmisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
