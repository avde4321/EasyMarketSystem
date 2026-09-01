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
    public DbSet<GeoRegionEntity> GeoRegiones => Set<GeoRegionEntity>();
    public DbSet<GeoProvinciaEntity> GeoProvincias => Set<GeoProvinciaEntity>();
    public DbSet<GeoCiudadEntity> GeoCiudades => Set<GeoCiudadEntity>();
    public DbSet<GeoSectorEntity> GeoSectores => Set<GeoSectorEntity>();

    public DbSet<SecurityRoleEntity> SecurityRoles => Set<SecurityRoleEntity>();
    public DbSet<SecurityPermisoEntity> SecurityPermisos => Set<SecurityPermisoEntity>();
    public DbSet<SecurityRolPermisoEntity> SecurityRolPermisos => Set<SecurityRolPermisoEntity>();
    public DbSet<SecurityAuditLogEntity> SecurityAuditLogs => Set<SecurityAuditLogEntity>();

    public DbSet<SecurityUserRoleEntity> SecurityUserRoles => Set<SecurityUserRoleEntity>();

    public DbSet<SecurityUserEmpresaEntity> SecurityUserEmpresas => Set<SecurityUserEmpresaEntity>();

    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();

    public DbSet<EmpleadoEntity> Empleados => Set<EmpleadoEntity>();
    public DbSet<ProveedorEntity> Proveedores => Set<ProveedorEntity>();
    public DbSet<ActivoFijoEntity> ActivosFijos => Set<ActivoFijoEntity>();
    public DbSet<CompraEntity> Compras => Set<CompraEntity>();
    public DbSet<CompraDetalleEntity> CompraDetalles => Set<CompraDetalleEntity>();
    public DbSet<FacturaCompraXmlLogEntity> FacturaCompraXmlLogs => Set<FacturaCompraXmlLogEntity>();
    public DbSet<EstudioMercadoCompraEntity> EstudiosMercadoCompra => Set<EstudioMercadoCompraEntity>();
    public DbSet<CuentaPorPagarEntity> CuentasPorPagar => Set<CuentaPorPagarEntity>();
    public DbSet<PagoCxPEntity> PagosCxP => Set<PagoCxPEntity>();
    public DbSet<MemoriaAnalisisFiscalEntity> MemoriasAnalisisFiscal => Set<MemoriaAnalisisFiscalEntity>();
    public DbSet<CuentaContableEntity> CuentasContables => Set<CuentaContableEntity>();
    public DbSet<PeriodoContableEntity> PeriodosContables => Set<PeriodoContableEntity>();
    public DbSet<AsientoContableEntity> AsientosContables => Set<AsientoContableEntity>();
    public DbSet<AsientoDetalleEntity> AsientosDetalle => Set<AsientoDetalleEntity>();

    public DbSet<EmpresaEmisoraEntity> EmpresasEmisoras => Set<EmpresaEmisoraEntity>();
    public DbSet<EmpresaPuntoEmisionEntity> EmpresaPuntosEmision => Set<EmpresaPuntoEmisionEntity>();
    public DbSet<CertificadoDigitalEmpresaEntity> CertificadosDigitalesEmpresa => Set<CertificadoDigitalEmpresaEntity>();
    public DbSet<EmpresaConfiguracionServiciosEntity> EmpresaConfiguracionServicios => Set<EmpresaConfiguracionServiciosEntity>();
    public DbSet<TransaccionPagoDigitalEntity> TransaccionesPagosDigitales => Set<TransaccionPagoDigitalEntity>();
    public DbSet<WhatsAppNotificacionLogEntity> WhatsAppNotificacionesLog => Set<WhatsAppNotificacionLogEntity>();

    public DbSet<PersonaEntity> Personas => Set<PersonaEntity>();
    public DbSet<PersonaNaturalEntity> PersonasNaturales => Set<PersonaNaturalEntity>();
    public DbSet<EmpresaClienteEntity> EmpresasCliente => Set<EmpresaClienteEntity>();

    public DbSet<ProductoEntity> Productos => Set<ProductoEntity>();
    public DbSet<BodegaEntity> Bodegas => Set<BodegaEntity>();
    public DbSet<ProductoBodegaEntity> ProductosBodega => Set<ProductoBodegaEntity>();

    public DbSet<KardexMovimientoEntity> KardexMovimientos => Set<KardexMovimientoEntity>();
    public DbSet<TransferenciaInventarioEntity> TransferenciasInventario => Set<TransferenciaInventarioEntity>();
    public DbSet<TransferenciaInventarioDetalleEntity> TransferenciasInventarioDetalle => Set<TransferenciaInventarioDetalleEntity>();

    public DbSet<FacturaEntity> Facturas => Set<FacturaEntity>();
    public DbSet<FacturaSecuencialEntity> FacturaSecuenciales => Set<FacturaSecuencialEntity>();
    public DbSet<CajaSesionEntity> CajaSesiones => Set<CajaSesionEntity>();

    public DbSet<FacturaDetalleEntity> FacturaDetalles => Set<FacturaDetalleEntity>();

    public DbSet<FacturaSriEventoEntity> FacturaSriEventos => Set<FacturaSriEventoEntity>();
    public DbSet<ComprobanteCabeceraEntity> ComprobanteCabecera => Set<ComprobanteCabeceraEntity>();
    public DbSet<ComprobanteDetalleEntity> ComprobanteDetalle => Set<ComprobanteDetalleEntity>();
    public DbSet<ColaProcesamientoSriEntity> ColaProcesamientoSRI => Set<ColaProcesamientoSriEntity>();
    public DbSet<DocumentoAdjuntoEntity> DocumentosAdjuntos => Set<DocumentoAdjuntoEntity>();
    public DbSet<ProformaEntity> Proformas => Set<ProformaEntity>();
    public DbSet<ProformaDetalleEntity> ProformaDetalles => Set<ProformaDetalleEntity>();
    public DbSet<ComprobanteRetencionEntity> ComprobantesRetencion => Set<ComprobanteRetencionEntity>();
    public DbSet<ComprobanteRetencionDetalleEntity> ComprobanteRetencionDetalles => Set<ComprobanteRetencionDetalleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDeIaDbContext).Assembly);

        modelBuilder.Entity<PersonaEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<PersonaNaturalEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<EmpresaClienteEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ClienteEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<EmpleadoEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ProveedorEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ActivoFijoEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<CompraEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<CompraDetalleEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.Compra.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<FacturaCompraXmlLogEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<EstudioMercadoCompraEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<CuentaPorPagarEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<PagoCxPEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.CuentaPorPagar.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<MemoriaAnalisisFiscalEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<CuentaContableEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<PeriodoContableEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<AsientoContableEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<AsientoDetalleEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.AsientoContable.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ProductoEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<BodegaEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ProductoBodegaEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<KardexMovimientoEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<TransferenciaInventarioEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<TransferenciaInventarioDetalleEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.TransferenciaInventario.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<FacturaEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<CajaSesionEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<FacturaDetalleEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.Factura.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<FacturaSriEventoEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.Factura.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<FacturaSecuencialEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ComprobanteCabeceraEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ComprobanteDetalleEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.ComprobanteCabecera.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ColaProcesamientoSriEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<DocumentoAdjuntoEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ProformaEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ProformaDetalleEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.Proforma.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ComprobanteRetencionEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<ComprobanteRetencionDetalleEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.ComprobanteRetencion.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<SecurityUserEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                entity.EmpresasAcceso.Any(link => link.EmpresaId == tenantContextAccessor.EmpresaId));
        modelBuilder.Entity<SecurityAuditLogEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<SecurityUserRoleEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                entity.User.EmpresasAcceso.Any(link => link.EmpresaId == tenantContextAccessor.EmpresaId));
        modelBuilder.Entity<SecurityUserEmpresaEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<EmpresaEmisoraEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                (tenantContextAccessor.UserId.HasValue && entity.UserAssignments.Any(link => link.SecurityUserId == tenantContextAccessor.UserId.Value)));
        modelBuilder.Entity<EmpresaPuntoEmisionEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                (tenantContextAccessor.UserId.HasValue && entity.Empresa.UserAssignments.Any(link => link.SecurityUserId == tenantContextAccessor.UserId.Value)));
        modelBuilder.Entity<CertificadoDigitalEmpresaEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                (tenantContextAccessor.UserId.HasValue && entity.Empresa.UserAssignments.Any(link => link.SecurityUserId == tenantContextAccessor.UserId.Value)));
        modelBuilder.Entity<EmpresaConfiguracionServiciosEntity>()
            .HasQueryFilter(entity =>
                tenantContextAccessor.IsSystemContext ||
                (tenantContextAccessor.UserId.HasValue && entity.Empresa.UserAssignments.Any(link => link.SecurityUserId == tenantContextAccessor.UserId.Value)));
        modelBuilder.Entity<TransaccionPagoDigitalEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
        modelBuilder.Entity<WhatsAppNotificacionLogEntity>()
            .HasQueryFilter(entity => tenantContextAccessor.IsSystemContext || entity.EmpresaId == tenantContextAccessor.EmpresaId);
    }
}



