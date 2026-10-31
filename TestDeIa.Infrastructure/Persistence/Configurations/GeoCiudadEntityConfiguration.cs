using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class GeoCiudadEntityConfiguration : IEntityTypeConfiguration<GeoCiudadEntity>
{
    public void Configure(EntityTypeBuilder<GeoCiudadEntity> builder)
    {
        builder.ToTable("GeoCiudades");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Codigo).HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Nombre).HasMaxLength(120).IsRequired();
        builder.HasIndex(entity => entity.Codigo).IsUnique();
        builder.HasOne(entity => entity.Provincia)
            .WithMany(provincia => provincia.Ciudades)
            .HasForeignKey(entity => entity.ProvinciaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
