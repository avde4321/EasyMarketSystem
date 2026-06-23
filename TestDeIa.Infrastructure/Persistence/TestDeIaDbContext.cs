using Microsoft.EntityFrameworkCore;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence;

public sealed class TestDeIaDbContext : DbContext
{
    public TestDeIaDbContext(DbContextOptions<TestDeIaDbContext> options)
        : base(options)
    {
    }

    public DbSet<SecurityUserEntity> SecurityUsers => Set<SecurityUserEntity>();

    public DbSet<SecurityRoleEntity> SecurityRoles => Set<SecurityRoleEntity>();

    public DbSet<SecurityUserRoleEntity> SecurityUserRoles => Set<SecurityUserRoleEntity>();

    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();

    public DbSet<EmpresaEmisoraEntity> EmpresasEmisoras => Set<EmpresaEmisoraEntity>();

    public DbSet<PersonaEntity> Personas => Set<PersonaEntity>();

    public DbSet<ProductoEntity> Productos => Set<ProductoEntity>();

    public DbSet<KardexMovimientoEntity> KardexMovimientos => Set<KardexMovimientoEntity>();

    public DbSet<FacturaEntity> Facturas => Set<FacturaEntity>();

    public DbSet<FacturaDetalleEntity> FacturaDetalles => Set<FacturaDetalleEntity>();

    public DbSet<FacturaSriEventoEntity> FacturaSriEventos => Set<FacturaSriEventoEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDeIaDbContext).Assembly);
    }
}
