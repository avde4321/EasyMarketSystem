using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence;

public sealed class TestDeIaDbContext : DbContext
{
    private readonly ITenantContextAccessor tenantContextAccessor;

    public TestDeIaDbContext(DbContextOptions<TestDeIaDbContext> options, ITenantContextAccessor tenantContextAccessor)
        : base(options)
    {
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public DbSet<SecurityUserEntity> SecurityUsers => Set<SecurityUserEntity>();
    public DbSet<CatalogoEntity> Catalogos => Set<CatalogoEntity>();
    public DbSet<CatalogoItemEntity> CatalogoItems => Set<CatalogoItemEntity>();

    public DbSet<SecurityRoleEntity> SecurityRoles => Set<SecurityRoleEntity>();

    public DbSet<SecurityUserRoleEntity> SecurityUserRoles => Set<SecurityUserRoleEntity>();

    public DbSet<SecurityUserEmpresaEntity> SecurityUserEmpresas => Set<SecurityUserEmpresaEntity>();

    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();

    public DbSet<EmpleadoEntity> Empleados => Set<EmpleadoEntity>();

    public DbSet<EmpresaEmisoraEntity> EmpresasEmisoras => Set<EmpresaEmisoraEntity>();
    public DbSet<EmpresaPuntoEmisionEntity> EmpresaPuntosEmision => Set<EmpresaPuntoEmisionEntity>();

    public DbSet<PersonaEntity> Personas => Set<PersonaEntity>();

    public DbSet<ProductoEntity> Productos => Set<ProductoEntity>();
    public DbSet<BodegaEntity> Bodegas => Set<BodegaEntity>();
    public DbSet<ProductoBodegaEntity> ProductosBodega => Set<ProductoBodegaEntity>();

    public DbSet<KardexMovimientoEntity> KardexMovimientos => Set<KardexMovimientoEntity>();

    public DbSet<FacturaEntity> Facturas => Set<FacturaEntity>();
    public DbSet<FacturaSecuencialEntity> FacturaSecuenciales => Set<FacturaSecuencialEntity>();

    public DbSet<FacturaDetalleEntity> FacturaDetalles => Set<FacturaDetalleEntity>();

    public DbSet<FacturaSriEventoEntity> FacturaSriEventos => Set<FacturaSriEventoEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDeIaDbContext).Assembly);

        modelBuilder.Entity<PersonaEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ClienteEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<EmpleadoEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ProductoEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<BodegaEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ProductoBodegaEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<KardexMovimientoEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<FacturaEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<FacturaDetalleEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.Factura.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<FacturaSriEventoEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.Factura.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<FacturaSecuencialEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<SecurityUserEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                entity.EmpresasAcceso.Any(link => link.EmpresaId == tenantContextAccessor.EmpresaId));
        modelBuilder.Entity<SecurityUserRoleEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                entity.User.EmpresasAcceso.Any(link => link.EmpresaId == tenantContextAccessor.EmpresaId));
        modelBuilder.Entity<SecurityUserEmpresaEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                (tenantContextAccessor.UserId.HasValue && entity.SecurityUserId == tenantContextAccessor.UserId.Value));
        modelBuilder.Entity<EmpresaEmisoraEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                (tenantContextAccessor.UserId.HasValue && entity.OwnerUserId == tenantContextAccessor.UserId.Value));
        modelBuilder.Entity<EmpresaPuntoEmisionEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                (tenantContextAccessor.UserId.HasValue && entity.Empresa.OwnerUserId == tenantContextAccessor.UserId.Value));
    }
}
