using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class EmpresaSmtpConfiguracionEntityConfiguration : IEntityTypeConfiguration<EmpresaSmtpConfiguracionEntity>
{
    public void Configure(EntityTypeBuilder<EmpresaSmtpConfiguracionEntity> builder)
    {
        builder.ToTable("EmpresaSmtpConfiguraciones");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.EmpresaId).IsRequired();
        builder.Property(entity => entity.Host).HasMaxLength(255).IsRequired();
        builder.Property(entity => entity.Puerto).IsRequired();
        builder.Property(entity => entity.Usuario).HasMaxLength(255);
        builder.Property(entity => entity.ContrasenaProtegida).HasMaxLength(4000);
        builder.Property(entity => entity.CorreoRemitente).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.NombreRemitente).HasMaxLength(180).IsRequired();
        builder.HasIndex(entity => entity.EmpresaId).IsUnique();
        builder.HasOne<EmpresaEmisoraEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
