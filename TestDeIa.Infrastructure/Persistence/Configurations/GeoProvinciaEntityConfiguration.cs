using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class GeoProvinciaEntityConfiguration : IEntityTypeConfiguration<GeoProvinciaEntity>
{
    public void Configure(EntityTypeBuilder<GeoProvinciaEntity> builder)
    {
        builder.ToTable("GeoProvincias");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Codigo).HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Nombre).HasMaxLength(120).IsRequired();
        builder.HasIndex(entity => entity.Codigo).IsUnique();
        builder.HasOne(entity => entity.Region)
            .WithMany(region => region.Provincias)
            .HasForeignKey(entity => entity.RegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
