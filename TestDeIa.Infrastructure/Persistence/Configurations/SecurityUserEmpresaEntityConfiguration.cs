using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SecurityUserEmpresaEntityConfiguration : IEntityTypeConfiguration<SecurityUserEmpresaEntity>
{
    public void Configure(EntityTypeBuilder<SecurityUserEmpresaEntity> builder)
    {
        builder.ToTable("SecurityUserEmpresas");

        builder.HasKey(current => new { current.SecurityUserId, current.EmpresaId });

        builder.HasOne(current => current.SecurityUser)
            .WithMany(user => user.EmpresasAcceso)
            .HasForeignKey(current => current.SecurityUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(current => current.Empresa)
            .WithMany(empresa => empresa.UserAssignments)
            .HasForeignKey(current => current.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(new SecurityUserEmpresaEntity
        {
            SecurityUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            EmpresaId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            IsDefault = true,
            CreatedAt = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero)
        });
    }
}
