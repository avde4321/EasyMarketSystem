using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class PersonaNaturalEntityConfiguration : IEntityTypeConfiguration<PersonaNaturalEntity>
{
    public void Configure(EntityTypeBuilder<PersonaNaturalEntity> builder)
    {
        builder.ToTable("PersonasNaturales");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.PrimerNombre).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.SegundoNombre).HasMaxLength(80);
        builder.Property(entity => entity.PrimerApellido).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.SegundoApellido).HasMaxLength(80);
        builder.Property(entity => entity.NumeroDocumento).HasMaxLength(13).IsRequired();
        builder.Property(entity => entity.Email).HasMaxLength(180);
        builder.Property(entity => entity.Telefono).HasMaxLength(40);
        builder.Property(entity => entity.Direccion).HasMaxLength(250).IsRequired();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.NumeroDocumento }).IsUnique();
    }
}
