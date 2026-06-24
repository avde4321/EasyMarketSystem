using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ClienteEntityConfiguration : IEntityTypeConfiguration<ClienteEntity>
{
    public void Configure(EntityTypeBuilder<ClienteEntity> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.EmpresaId)
            .IsRequired();

        builder.HasIndex(cliente => new { cliente.EmpresaId, cliente.PersonaId })
            .IsUnique();

        builder.HasOne(cliente => cliente.Persona)
            .WithOne(persona => persona.Cliente)
            .HasForeignKey<ClienteEntity>(cliente => cliente.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
