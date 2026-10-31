using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class EfComisionesRepository : IComisionesRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public EfComisionesRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<LiquidacionComisionResponse> GetLiquidacionAsync(DateOnly desde, DateOnly hasta, Guid? operadorId = null, CancellationToken cancellationToken = default)
    {
        var empresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para consultar comisiones.");
        var desdeDate = desde.ToDateTime(TimeOnly.MinValue);
        var hastaDate = hasta.ToDateTime(TimeOnly.MaxValue);

        var query =
            from detalle in dbContext.FacturaDetalles.AsNoTracking()
            join factura in dbContext.Facturas.AsNoTracking() on detalle.FacturaId equals factura.Id
            join producto in dbContext.Productos.AsNoTracking() on detalle.ProductoId equals producto.Id
            join usuario in dbContext.SecurityUsers.AsNoTracking().Include(current => current.Persona) on detalle.UsuarioIdOperador equals usuario.Id
            where factura.EmpresaId == empresaId &&
                  factura.FechaEmision >= desdeDate &&
                  factura.FechaEmision <= hastaDate &&
                  factura.Estado == FacturaEstado.AUTORIZADO &&
                  !producto.ControlaStock &&
                  detalle.UsuarioIdOperador.HasValue &&
                  detalle.MontoComisionCalculado > 0
            select new { detalle, factura, usuario };

        if (operadorId.HasValue && operadorId.Value != Guid.Empty)
        {
            query = query.Where(current => current.detalle.UsuarioIdOperador == operadorId.Value);
        }

        var rows = await query
            .OrderBy(current => current.factura.FechaEmision)
            .ThenBy(current => current.factura.Secuencial)
            .ToListAsync(cancellationToken);

        var detalles = rows.Select(current => new LiquidacionComisionDetalleResponse
        {
            FacturaId = current.factura.Id,
            DetalleId = current.detalle.Id,
            FechaEmision = current.factura.FechaEmision,
            NumeroFactura = $"{current.factura.Establecimiento}-{current.factura.PuntoEmision}-{current.factura.Secuencial}",
            OperadorId = current.usuario.Id,
            Operador = string.IsNullOrWhiteSpace(current.usuario.DisplayName) ? current.usuario.UserName : current.usuario.DisplayName,
            IdentificacionOperador = current.usuario.Persona.Identificacion,
            Servicio = current.detalle.NombreProducto,
            Cantidad = current.detalle.Cantidad,
            Subtotal = current.detalle.Subtotal,
            MontoComision = current.detalle.MontoComisionCalculado
        }).ToArray();

        return new LiquidacionComisionResponse
        {
            Desde = desde,
            Hasta = hasta,
            OperadorId = operadorId,
            TotalServicios = detalles.Sum(current => current.Subtotal),
            TotalComisiones = detalles.Sum(current => current.MontoComision),
            Detalles = detalles
        };
    }
}