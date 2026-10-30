using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SaasInicializacionEntityConfiguration : IEntityTypeConfiguration<SaasInicializacionEntity>
{
    public void Configure(EntityTypeBuilder<SaasInicializacionEntity> b)
    {
        b.ToTable("SaasInicializaciones");
        b.HasKey(x => x.Clave);
        b.Property(x => x.Clave).HasMaxLength(50).IsUnicode(false);
    }
}
