using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class GeoRegionEntityConfiguration : IEntityTypeConfiguration<GeoRegionEntity>
{
    public void Configure(EntityTypeBuilder<GeoRegionEntity> builder)
    {
        builder.ToTable("GeoRegiones");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Codigo).HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Nombre).HasMaxLength(120).IsRequired();
        builder.HasIndex(entity => entity.Codigo).IsUnique();
    }
}
