using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class GeoSectorEntityConfiguration : IEntityTypeConfiguration<GeoSectorEntity>
{
    public void Configure(EntityTypeBuilder<GeoSectorEntity> builder)
    {
        builder.ToTable("GeoSectores");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Codigo).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Nombre).HasMaxLength(160).IsRequired();
        builder.HasIndex(entity => new { entity.CiudadId, entity.Codigo }).IsUnique();
        builder.HasOne(entity => entity.Ciudad)
            .WithMany(ciudad => ciudad.Sectores)
            .HasForeignKey(entity => entity.CiudadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
