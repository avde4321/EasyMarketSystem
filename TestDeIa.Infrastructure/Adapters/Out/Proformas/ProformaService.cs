using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Application.Modules.Proformas.Ports.In;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Proformas.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Requests.Proformas;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Facturacion;
using TestDeIa.Shared.Responses.Proformas;

namespace TestDeIa.Infrastructure.Adapters.Out.Proformas;

public sealed class ProformaService(
    TestDeIaDbContext dbContext,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserAccessor currentUserAccessor,
    IFacturacionUseCase facturacionUseCase,
    ILogger<ProformaService> logger) : IProformaService
{
    public async Task<PagedResultResponse<ProformaResponse>> GetPagedAsync(
        string? term,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Proformas
            .AsNoTracking()
            .AsQueryable();

        var normalizedTerm = term?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedTerm))
        {
            query = query.Where(current =>
                current.Secuencial.Contains(normalizedTerm) ||
                (current.Observacion != null && current.Observacion.Contains(normalizedTerm)));
        }

        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(current => current.FechaEmision)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);

        return new PagedResultResponse<ProformaResponse>
        {
            Items = entities.Select(Map).ToArray(),
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public async Task<ProformaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Proformas
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<ProformaResponse> CreateAsync(ProformaRequest request, CancellationToken cancellationToken = default)
    {
        var empresaId = ResolveEmpresaId();
        ValidateRequest(request);

        await EnsureReferencesAsync(empresaId, request.ClienteId, request.UsuarioId, request.BodegaId, request.Detalles, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new ProformaEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ClienteId = request.ClienteId,
            UsuarioId = request.UsuarioId == Guid.Empty ? currentUserAccessor.GetRequiredUserId() : request.UsuarioId,
            BodegaId = request.BodegaId,
            Secuencial = string.IsNullOrWhiteSpace(request.Secuencial)
                ? await GenerateNextSecuencialAsync(empresaId, cancellationToken)
                : NormalizeSecuencial(request.Secuencial),
            FechaEmision = new DateTimeOffset(request.FechaEmision.Date, TimeSpan.Zero),
            FechaVencimiento = request.FechaVencimiento.HasValue
                ? new DateTimeOffset(request.FechaVencimiento.Value.Date, TimeSpan.Zero)
                : null,
            Estado = request.Estado == 0 ? EstadoProforma.Pendiente : ParseEstado(request.Estado),
            SubtotalSinImpuestos = Round(request.SubtotalSinImpuestos),
            SubtotalIVA = Round(request.SubtotalIVA),
            DescuentoTotal = Round(request.DescuentoTotal),
            Total = Round(request.Total),
            Observacion = NormalizeOptional(request.Observacion),
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var detail in request.Detalles)
        {
            entity.Detalles.Add(new ProformaDetalleEntity
            {
                Id = Guid.NewGuid(),
                ProformaId = entity.Id,
                ProductoId = detail.ProductoId,
                Cantidad = Round(detail.Cantidad),
                PrecioUnitario = Round(detail.PrecioUnitario),
                Descuento = Round(detail.Descuento),
                TarifaIVA = Round(detail.TarifaIVA),
                ValorIVA = Round(detail.ValorIVA),
                Subtotal = Round(detail.Subtotal)
            });
        }

        dbContext.Proformas.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<ProformaResponse?> UpdateAsync(Guid id, ProformaRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var entity = await dbContext.Proformas
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Estado == EstadoProforma.Facturada)
        {
            throw new InvalidOperationException("No se puede editar una proforma ya facturada.");
        }

        if (entity.Estado == EstadoProforma.Anulada)
        {
            throw new InvalidOperationException("No se puede editar una proforma anulada.");
        }

        await EnsureReferencesAsync(entity.EmpresaId, request.ClienteId, request.UsuarioId, request.BodegaId, request.Detalles, cancellationToken);

        entity.ClienteId = request.ClienteId;
        entity.UsuarioId = request.UsuarioId == Guid.Empty ? entity.UsuarioId : request.UsuarioId;
        entity.BodegaId = request.BodegaId;
        entity.FechaEmision = new DateTimeOffset(request.FechaEmision.Date, TimeSpan.Zero);
        entity.FechaVencimiento = request.FechaVencimiento.HasValue
            ? new DateTimeOffset(request.FechaVencimiento.Value.Date, TimeSpan.Zero)
            : null;
        entity.Estado = ParseEstado(request.Estado);
        entity.SubtotalSinImpuestos = Round(request.SubtotalSinImpuestos);
        entity.SubtotalIVA = Round(request.SubtotalIVA);
        entity.DescuentoTotal = Round(request.DescuentoTotal);
        entity.Total = Round(request.Total);
        entity.Observacion = NormalizeOptional(request.Observacion);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.ProformaDetalles.RemoveRange(entity.Detalles);
        entity.Detalles.Clear();

        foreach (var detail in request.Detalles)
        {
            entity.Detalles.Add(new ProformaDetalleEntity
            {
                Id = Guid.NewGuid(),
                ProformaId = entity.Id,
                ProductoId = detail.ProductoId,
                Cantidad = Round(detail.Cantidad),
                PrecioUnitario = Round(detail.PrecioUnitario),
                Descuento = Round(detail.Descuento),
                TarifaIVA = Round(detail.TarifaIVA),
                ValorIVA = Round(detail.ValorIVA),
                Subtotal = Round(detail.Subtotal)
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<bool> AnularAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Proformas.FirstOrDefaultAsync(current => current.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        if (entity.Estado == EstadoProforma.Facturada)
        {
            throw new InvalidOperationException("No se puede anular una proforma ya facturada.");
        }

        entity.Estado = EstadoProforma.Anulada;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<FacturaEmissionResponse> ConvertirProformaAFacturaAsync(
        ConvertirProformaAFacturaDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProformaId == Guid.Empty)
        {
            throw new InvalidOperationException("Debe seleccionar una proforma valida.");
        }

        var proforma = await dbContext.Proformas
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == request.ProformaId, cancellationToken)
            ?? throw new InvalidOperationException("La proforma no existe o no pertenece a la empresa activa.");

        if (proforma.Estado != EstadoProforma.Pendiente)
        {
            throw new InvalidOperationException("Solo se pueden convertir proformas en estado Pendiente.");
        }

        if (proforma.FacturaId.HasValue)
        {
            throw new InvalidOperationException("La proforma ya se encuentra vinculada a una factura.");
        }

        var punto = await dbContext.EmpresaPuntosEmision
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == request.PuntoEmisionId, cancellationToken)
            ?? throw new InvalidOperationException("El punto de emision seleccionado no esta disponible.");

        if (punto.BodegaId != proforma.BodegaId)
        {
            throw new InvalidOperationException("El punto de emision seleccionado no corresponde a la bodega de la proforma.");
        }

        await EnsureStockAsync(proforma, cancellationToken);

        var facturaRequest = new EmitirFacturaRequest
        {
            ClienteId = proforma.ClienteId,
            BodegaId = proforma.BodegaId,
            Establecimiento = punto.Establecimiento,
            PuntoEmision = punto.PuntoEmision,
            FormaPago = request.FormaPagoId,
            Observacion = proforma.Observacion,
            Items = proforma.Detalles.Select(detail => new EmitirFacturaDetalleRequest
            {
                ProductoId = detail.ProductoId,
                Cantidad = detail.Cantidad,
                Descuento = detail.Descuento,
                PrecioUnitarioOverride = detail.PrecioUnitario
            }).ToArray()
        };

        logger.LogInformation(
            "Convirtiendo proforma {ProformaId} a factura. EmpresaId={EmpresaId}, BodegaId={BodegaId}",
            proforma.Id,
            proforma.EmpresaId,
            proforma.BodegaId);

        var factura = await facturacionUseCase.EmitirFacturaAsync(facturaRequest, cancellationToken);

        var updated = await dbContext.Proformas
            .FirstAsync(current => current.Id == request.ProformaId, cancellationToken);
        updated.Estado = EstadoProforma.Facturada;
        updated.FacturaId = factura.FacturaId;
        updated.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Proforma {ProformaId} convertida a factura {FacturaId}.",
            updated.Id,
            factura.FacturaId);

        return factura;
    }

    private Guid ResolveEmpresaId()
    {
        return tenantContextAccessor.EmpresaId
            ?? currentUserAccessor.GetRequiredEmpresaId();
    }

    private async Task<string> GenerateNextSecuencialAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        var lastSecuencial = await dbContext.Proformas
            .IgnoreQueryFilters()
            .Where(current => current.EmpresaId == empresaId)
            .OrderByDescending(current => current.Secuencial)
            .Select(current => current.Secuencial)
            .FirstOrDefaultAsync(cancellationToken);

        var next = long.TryParse(lastSecuencial, out var last)
            ? last + 1
            : 1;

        return next.ToString("000000000");
    }

    private async Task EnsureStockAsync(ProformaEntity proforma, CancellationToken cancellationToken)
    {
        var productIds = proforma.Detalles.Select(current => current.ProductoId).Distinct().ToArray();
        var productos = await dbContext.Productos
            .AsNoTracking()
            .Where(current => productIds.Contains(current.Id))
            .ToDictionaryAsync(current => current.Id, cancellationToken);

        var stocks = await dbContext.ProductosBodega
            .AsNoTracking()
            .Where(current => current.BodegaId == proforma.BodegaId && productIds.Contains(current.ProductoId))
            .ToDictionaryAsync(current => current.ProductoId, cancellationToken);

        foreach (var detail in proforma.Detalles)
        {
            if (!productos.TryGetValue(detail.ProductoId, out var producto))
            {
                throw new InvalidOperationException("La proforma contiene productos no disponibles.");
            }

            if (!producto.ControlaStock)
            {
                continue;
            }

            var stock = stocks.TryGetValue(detail.ProductoId, out var existencia)
                ? existencia.StockActual
                : 0m;

            if (stock < detail.Cantidad)
            {
                throw new StockInsuficienteException($"No hay stock suficiente para {producto.Nombre}. Disponible: {stock:0.####}. Requerido: {detail.Cantidad:0.####}.");
            }
        }
    }

    private async Task EnsureReferencesAsync(
        Guid empresaId,
        Guid clienteId,
        Guid usuarioId,
        Guid bodegaId,
        IReadOnlyCollection<ProformaDetalleRequest> detalles,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Clientes.AnyAsync(current => current.EmpresaId == empresaId && current.PersonaId == clienteId, cancellationToken))
        {
            throw new InvalidOperationException("El cliente seleccionado no existe en la empresa activa.");
        }

        if (usuarioId != Guid.Empty &&
            !await dbContext.SecurityUsers.AnyAsync(current => current.Id == usuarioId, cancellationToken))
        {
            throw new InvalidOperationException("El usuario seleccionado no existe.");
        }

        if (!await dbContext.Bodegas.AnyAsync(current => current.EmpresaId == empresaId && current.Id == bodegaId && current.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("La bodega seleccionada no esta activa.");
        }

        var productIds = detalles.Select(current => current.ProductoId).Distinct().ToArray();
        var productCount = await dbContext.Productos.CountAsync(current => productIds.Contains(current.Id) && current.IsActive, cancellationToken);
        if (productCount != productIds.Length)
        {
            throw new InvalidOperationException("Uno o varios productos de la proforma no existen o estan inactivos.");
        }
    }

    private static void ValidateRequest(ProformaRequest request)
    {
        if (request.ClienteId == Guid.Empty)
        {
            throw new InvalidOperationException("Debe seleccionar un cliente para la proforma.");
        }

        if (request.BodegaId == Guid.Empty)
        {
            throw new InvalidOperationException("Debe seleccionar una bodega para la proforma.");
        }

        if (request.Detalles.Count == 0)
        {
            throw new InvalidOperationException("La proforma debe tener al menos un detalle.");
        }

        if (request.Detalles.Any(current => current.ProductoId == Guid.Empty || current.Cantidad <= 0 || current.PrecioUnitario < 0 || current.Descuento < 0))
        {
            throw new InvalidOperationException("La proforma contiene detalles invalidos.");
        }
    }

    private static EstadoProforma ParseEstado(int estado)
    {
        return Enum.IsDefined(typeof(EstadoProforma), estado)
            ? (EstadoProforma)estado
            : throw new InvalidOperationException("El estado de la proforma no es valido.");
    }

    private static string NormalizeSecuencial(string? secuencial)
    {
        var normalized = secuencial?.Trim() ?? string.Empty;
        if (normalized.Length > 9 || !normalized.All(char.IsDigit))
        {
            throw new InvalidOperationException("El secuencial de proforma debe contener hasta 9 digitos.");
        }

        return normalized.PadLeft(9, '0');
    }

    private static decimal Round(decimal value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static ProformaResponse Map(ProformaEntity entity)
    {
        return new ProformaResponse
        {
            Id = entity.Id,
            EmpresaId = entity.EmpresaId,
            ClienteId = entity.ClienteId,
            UsuarioId = entity.UsuarioId,
            BodegaId = entity.BodegaId,
            Secuencial = entity.Secuencial,
            FechaEmision = entity.FechaEmision.Date,
            FechaVencimiento = entity.FechaVencimiento?.Date,
            Estado = (int)entity.Estado,
            EstadoNombre = entity.Estado.ToString(),
            SubtotalSinImpuestos = entity.SubtotalSinImpuestos,
            SubtotalIVA = entity.SubtotalIVA,
            DescuentoTotal = entity.DescuentoTotal,
            Total = entity.Total,
            Observacion = entity.Observacion,
            FacturaId = entity.FacturaId,
            Detalles = entity.Detalles.Select(detail => new ProformaDetalleResponse
            {
                Id = detail.Id,
                ProductoId = detail.ProductoId,
                Cantidad = detail.Cantidad,
                PrecioUnitario = detail.PrecioUnitario,
                Descuento = detail.Descuento,
                TarifaIVA = detail.TarifaIVA,
                ValorIVA = detail.ValorIVA,
                Subtotal = detail.Subtotal
            }).ToArray()
        };
    }
}
