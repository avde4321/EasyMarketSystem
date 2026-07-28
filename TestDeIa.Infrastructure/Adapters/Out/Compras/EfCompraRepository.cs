using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Application.Modules.ActivosFijos.UseCases;
using TestDeIa.Domain.Modules.Compras.Entities;
using TestDeIa.Domain.Modules.Compras.Enums;
using TestDeIa.Domain.Modules.ActivosFijos.Enums;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using CompraDocumentTypes = TestDeIa.Shared.Compras.CompraDocumentTypes;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class EfCompraRepository : ICompraRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly IInventarioRepository inventarioRepository;
    private readonly ActivoFijoService activoFijoService;

    public EfCompraRepository(TestDeIaDbContext dbContext, IInventarioRepository inventarioRepository, ActivoFijoService activoFijoService)
    {
        this.dbContext = dbContext;
        this.inventarioRepository = inventarioRepository;
        this.activoFijoService = activoFijoService;
    }

    public async Task<Compra> CreateAsync(Compra compra, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var secuencial = compra.Secuencial;
        string? claveAccesoGenerada = compra.ClaveAccesoGenerada;
        string? xmlGenerado = compra.XmlGenerado;
        string? mensajeEstado = compra.MensajeEstado;
        FacturaEstado? estadoSri = compra.EstadoSri;

        if (compra.TipoDocumentoCodigo == CompraDocumentTypes.LiquidacionCompra)
        {
            secuencial = await ReserveNextSecuencialAsync(
                compra.EmpresaId,
                compra.TipoDocumentoCodigo,
                compra.Establecimiento,
                compra.PuntoEmision,
                compra.CreatedAt,
                cancellationToken);

            var empresa = await dbContext.EmpresasEmisoras
                .Include(current => current.PuntosEmision)
                .FirstOrDefaultAsync(current => current.Id == compra.EmpresaId && current.IsActive, cancellationToken)
                ?? throw new InvalidOperationException("No existe una empresa activa para emitir la liquidacion.");

            var proveedor = await dbContext.Proveedores
                .Include(current => current.Persona)
                .FirstOrDefaultAsync(current => current.PersonaId == compra.ProveedorId && current.EmpresaId == compra.EmpresaId, cancellationToken)
                ?? throw new InvalidOperationException("El proveedor seleccionado no pertenece a la empresa activa.");

            var tempEntity = BuildEntity(compra, secuencial, claveAccesoGenerada, xmlGenerado, mensajeEstado, estadoSri);
            claveAccesoGenerada = SriLiquidacionCompraXmlBuilder.GenerateClaveAcceso(tempEntity, empresa);
            tempEntity.ClaveAccesoGenerada = claveAccesoGenerada;
            xmlGenerado = SriLiquidacionCompraXmlBuilder.BuildUnsignedXml(tempEntity, empresa, proveedor);
            mensajeEstado = "Liquidacion registrada y en cola para firma electronica.";
            estadoSri = FacturaEstado.PENDIENTE;
        }

        var entity = BuildEntity(compra, secuencial, claveAccesoGenerada, xmlGenerado, mensajeEstado, estadoSri);
        dbContext.Compras.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var detalle in compra.Detalles)
        {
            if (detalle.Cantidad <= 0)
            {
                continue;
            }

            if (detalle.NaturalezaCompra == NaturalezaCompra.MercaderiaInventario &&
                detalle.ProductoId.HasValue)
            {
                await inventarioRepository.RegistrarCompraAsync(
                    detalle.ProductoId.Value,
                    compra.BodegaId,
                    detalle.Cantidad,
                    detalle.CostoUnitario,
                    $"{CompraDocumentTypes.GetName(compra.TipoDocumentoCodigo).ToUpperInvariant()} {entity.Establecimiento}-{entity.PuntoEmision}-{entity.Secuencial}",
                    cancellationToken);
            }
        }

        await CreateActivosFijosAsync(entity, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var persisted = await dbContext.Compras
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstAsync(current => current.Id == compra.Id, cancellationToken);

        return Map(persisted);
    }

    private async Task CreateActivosFijosAsync(CompraEntity compra, CancellationToken cancellationToken)
    {
        var detallesActivoFijo = compra.Detalles
            .Where(detalle => detalle.NaturalezaCompra == NaturalezaCompra.ActivoFijo)
            .ToArray();

        if (detallesActivoFijo.Length == 0)
        {
            return;
        }

        var currentCount = await dbContext.ActivosFijos
            .IgnoreQueryFilters()
            .CountAsync(current => current.EmpresaId == compra.EmpresaId && current.CodigoActivo.StartsWith($"AF-{compra.FechaEmision.Year}-"), cancellationToken);

        foreach (var detalle in detallesActivoFijo)
        {
            var categoria = activoFijoService.ResolveCategoriaFromSriCode(detalle.CategoriaSriActivo);
            var parametros = activoFijoService.GetParametrosSri(categoria);
            currentCount++;

            dbContext.ActivosFijos.Add(new ActivoFijoEntity
            {
                Id = Guid.NewGuid(),
                EmpresaId = compra.EmpresaId,
                CompraDetalleId = detalle.Id,
                CodigoActivo = $"AF-{compra.FechaEmision.Year}-{currentCount:000}",
                Nombre = string.IsNullOrWhiteSpace(detalle.NombreActivo) ? detalle.ProductoNombre : detalle.NombreActivo.Trim(),
                SerieMarca = detalle.SerieUbicacionActivo,
                CategoriaSRI = categoria,
                FechaAdquisicion = compra.FechaEmision.Date,
                CostoInicial = detalle.CostoTotalSinImpuesto,
                ValorResidual = 0m,
                VidaUtilAnios = parametros.VidaUtilAnios,
                PorcentajeDepreciacionAnual = parametros.PorcentajeDepreciacionAnual,
                UbicacionFisica = detalle.SerieUbicacionActivo,
                CustodioResponsable = null,
                EstadoActivo = EstadoActivoFijo.Activo,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetPendingLiquidacionIdsAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        return await dbContext.Compras
            .AsNoTracking()
            .Where(current =>
                current.TipoDocumentoCodigo == CompraDocumentTypes.LiquidacionCompra &&
                current.EstadoSri == FacturaEstado.PENDIENTE &&
                current.XmlFirmado == null &&
                current.ClaveAccesoGenerada != null &&
                (current.NextRetryAt == null || current.NextRetryAt <= now))
            .OrderBy(current => current.CreatedAt)
            .Select(current => current.Id)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Compra?> TryClaimLiquidacionAsync(Guid compraId, string workerId, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var updatedRows = await dbContext.Compras
            .Where(current =>
                current.Id == compraId &&
                current.TipoDocumentoCodigo == CompraDocumentTypes.LiquidacionCompra &&
                current.EstadoSri == FacturaEstado.PENDIENTE &&
                current.XmlFirmado == null &&
                (current.NextRetryAt == null || current.NextRetryAt <= now) &&
                (current.ProcessingStartedAt == null || current.ProcessingStartedAt <= now.AddMinutes(-2)))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(current => current.ProcessingNode, workerId)
                .SetProperty(current => current.ProcessingStartedAt, now)
                .SetProperty(current => current.RetryCount, current => current.RetryCount + 1)
                .SetProperty(current => current.NextRetryAt, (DateTimeOffset?)null),
                cancellationToken);

        if (updatedRows == 0)
        {
            return null;
        }

        var entity = await dbContext.Compras
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstAsync(current => current.Id == compraId, cancellationToken);

        return Map(entity);
    }

    public Task MarkLiquidacionAsAuthorizedAsync(
        Guid compraId,
        string claveAcceso,
        string? numeroAutorizacion,
        string xmlFirmado,
        string mensaje,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default)
    {
        return UpdateSriStateAsync(
            compraId,
            FacturaEstado.AUTORIZADO,
            claveAcceso,
            numeroAutorizacion,
            xmlFirmado,
            xmlFirmado,
            mensaje,
            null,
            fechaRespuesta,
            cancellationToken);
    }

    public Task MarkLiquidacionAsSignedPendingAsync(
        Guid compraId,
        string claveAcceso,
        string xmlFirmado,
        string mensaje,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default)
    {
        return UpdateSriStateAsync(
            compraId,
            FacturaEstado.PENDIENTE,
            claveAcceso,
            null,
            null,
            xmlFirmado,
            mensaje,
            null,
            fechaRespuesta,
            cancellationToken);
    }

    public Task MarkLiquidacionAsUnsignedAsync(
        Guid compraId,
        string claveAcceso,
        string mensaje,
        string xmlGenerado,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default)
    {
        return UpdateSriStateAsync(
            compraId,
            FacturaEstado.NO_FIRMADO,
            claveAcceso,
            null,
            xmlGenerado,
            null,
            mensaje,
            null,
            fechaRespuesta,
            cancellationToken);
    }

    public Task MarkLiquidacionAsRejectedAsync(
        Guid compraId,
        string mensaje,
        string? xmlFirmado,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default)
    {
        return UpdateSriStateAsync(
            compraId,
            FacturaEstado.RECHAZADO,
            null,
            null,
            null,
            xmlFirmado,
            mensaje,
            null,
            fechaRespuesta,
            cancellationToken);
    }

    public Task MarkLiquidacionAsErrorAsync(
        Guid compraId,
        string mensaje,
        DateTimeOffset nextRetryAt,
        CancellationToken cancellationToken = default)
    {
        return UpdateSriStateAsync(
            compraId,
            null,
            null,
            null,
            null,
            null,
            mensaje,
            nextRetryAt,
            DateTimeOffset.UtcNow,
            cancellationToken);
    }

    private async Task UpdateSriStateAsync(
        Guid compraId,
        FacturaEstado? estadoSri,
        string? claveAcceso,
        string? numeroAutorizacion,
        string? xmlGenerado,
        string? xmlFirmado,
        string mensaje,
        DateTimeOffset? nextRetryAt,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        var compra = await dbContext.Compras.FirstAsync(current => current.Id == compraId, cancellationToken);

        if (estadoSri.HasValue)
        {
            compra.EstadoSri = estadoSri.Value;
        }

        if (!string.IsNullOrWhiteSpace(claveAcceso))
        {
            compra.ClaveAccesoGenerada = claveAcceso;
        }

        if (!string.IsNullOrWhiteSpace(numeroAutorizacion))
        {
            compra.NumeroAutorizacion = numeroAutorizacion;
        }

        if (!string.IsNullOrWhiteSpace(xmlGenerado))
        {
            compra.XmlGenerado = xmlGenerado;
        }

        if (!string.IsNullOrWhiteSpace(xmlFirmado))
        {
            compra.XmlFirmado = xmlFirmado;
        }

        compra.MensajeEstado = mensaje;
        compra.ProcessingNode = null;
        compra.ProcessingStartedAt = null;
        compra.NextRetryAt = nextRetryAt;
        compra.UpdatedAt = updatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private CompraEntity BuildEntity(
        Compra compra,
        string secuencial,
        string? claveAccesoGenerada,
        string? xmlGenerado,
        string? mensajeEstado,
        FacturaEstado? estadoSri)
    {
        return new CompraEntity
        {
            Id = compra.Id,
            EmpresaId = compra.EmpresaId,
            ProveedorId = compra.ProveedorId,
            BodegaId = compra.BodegaId,
            NaturalezaCompra = compra.NaturalezaCompra,
            TipoDocumentoCodigo = compra.TipoDocumentoCodigo,
            TipoComprobanteSRI = compra.TipoComprobanteSRI,
            SustentoTributarioSRI = compra.SustentoTributarioSRI,
            Establecimiento = compra.Establecimiento,
            PuntoEmision = compra.PuntoEmision,
            Secuencial = secuencial,
            ClaveAccesoProveedor = compra.ClaveAccesoProveedor,
            ClaveAccesoGenerada = claveAccesoGenerada,
            NumeroAutorizacion = compra.NumeroAutorizacion,
            EstadoSri = estadoSri,
            MensajeEstado = mensajeEstado,
            FormaPagoSriCodigo = compra.FormaPagoSriCodigo,
            FormaPagoCompra = compra.FormaPagoCompra,
            RequiereBancarizacion = compra.RequiereBancarizacion,
            Observacion = compra.Observacion,
            XmlGenerado = xmlGenerado,
            XmlFirmado = compra.XmlFirmado,
            ProcessingNode = compra.ProcessingNode,
            ProcessingStartedAt = compra.ProcessingStartedAt,
            RetryCount = compra.RetryCount,
            NextRetryAt = compra.NextRetryAt,
            FechaEmision = compra.FechaEmision,
            SubtotalIva0 = compra.SubtotalIva0,
            SubtotalIva5 = compra.SubtotalIva5,
            SubtotalIva8 = compra.SubtotalIva8,
            SubtotalIva15 = compra.SubtotalIva15,
            TotalDescuento = compra.TotalDescuento,
            TotalImpuestos = compra.TotalImpuestos,
            ImporteTotal = compra.ImporteTotal,
            EstadoCompra = compra.EstadoCompra.ToString(),
            CreatedAt = compra.CreatedAt,
            UsuarioCreacionId = compra.UsuarioCreacionId,
            UpdatedAt = compra.UpdatedAt,
            UsuarioModificacionId = compra.UsuarioModificacionId,
            Detalles = compra.Detalles.Select(detalle => new CompraDetalleEntity
            {
                Id = detalle.Id,
                EmpresaId = compra.EmpresaId,
                CompraId = compra.Id,
                FechaEmisionCompra = compra.FechaEmision,
                ProductoId = detalle.ProductoId,
                ProductoCodigo = detalle.ProductoCodigo,
                ProductoNombre = detalle.ProductoNombre,
                NaturalezaCompra = detalle.NaturalezaCompra,
                NombreActivo = detalle.NombreActivo,
                CategoriaSriActivo = detalle.CategoriaSriActivo,
                SerieUbicacionActivo = detalle.SerieUbicacionActivo,
                CodigoIva = detalle.CodigoIva,
                PorcentajeIva = detalle.PorcentajeIva,
                Cantidad = detalle.Cantidad,
                CostoUnitario = detalle.CostoUnitario,
                Descuento = detalle.Descuento,
                CostoTotalSinImpuesto = detalle.CostoTotalSinImpuesto
            }).ToArray()
        };
    }

    private async Task<string> ReserveNextSecuencialAsync(
        Guid empresaId,
        string codigoDocumento,
        string establecimiento,
        string puntoEmision,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var record = await dbContext.FacturaSecuenciales
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.CodigoDocumento == codigoDocumento &&
                current.Establecimiento == establecimiento &&
                current.PuntoEmision == puntoEmision,
                cancellationToken);

        if (record is null)
        {
            record = new FacturaSecuencialEntity
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                CodigoDocumento = codigoDocumento,
                Establecimiento = establecimiento,
                PuntoEmision = puntoEmision,
                UltimoSecuencial = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
            dbContext.FacturaSecuenciales.Add(record);
        }
        else
        {
            record.UltimoSecuencial += 1;
            record.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return record.UltimoSecuencial.ToString("000000000");
    }

    private static Compra Map(CompraEntity entity)
    {
        return new Compra(
            entity.Id,
            entity.EmpresaId,
            entity.ProveedorId,
            entity.BodegaId,
            entity.NaturalezaCompra,
            entity.TipoDocumentoCodigo,
            entity.TipoComprobanteSRI,
            entity.SustentoTributarioSRI,
            entity.Establecimiento,
            entity.PuntoEmision,
            entity.Secuencial,
            entity.ClaveAccesoProveedor,
            entity.ClaveAccesoGenerada,
            entity.NumeroAutorizacion,
            entity.EstadoSri,
            entity.MensajeEstado,
            entity.FormaPagoSriCodigo,
            entity.FormaPagoCompra,
            entity.RequiereBancarizacion,
            entity.Observacion,
            entity.XmlGenerado,
            entity.XmlFirmado,
            entity.ProcessingNode,
            entity.ProcessingStartedAt,
            entity.RetryCount,
            entity.NextRetryAt,
            entity.FechaEmision,
            entity.SubtotalIva0,
            entity.SubtotalIva5,
            entity.SubtotalIva8,
            entity.SubtotalIva15,
            entity.TotalDescuento,
            entity.TotalImpuestos,
            entity.ImporteTotal,
            ParseEstado(entity.EstadoCompra),
            entity.CreatedAt,
            entity.UsuarioCreacionId,
            entity.UpdatedAt,
            entity.UsuarioModificacionId,
            entity.Detalles
                .OrderBy(detalle => detalle.ProductoNombre)
                .Select(detalle => new CompraDetalle(
                    detalle.Id,
                    detalle.CompraId,
                    detalle.ProductoId,
                    detalle.ProductoCodigo,
                    detalle.ProductoNombre,
                    detalle.NaturalezaCompra,
                    detalle.NombreActivo,
                    detalle.CategoriaSriActivo,
                    detalle.SerieUbicacionActivo,
                    detalle.CodigoIva,
                    detalle.PorcentajeIva,
                    detalle.Cantidad,
                    detalle.CostoUnitario,
                    detalle.Descuento,
                    detalle.CostoTotalSinImpuesto))
                .ToArray());
    }

    private static EstadoCompra ParseEstado(string value)
    {
        return Enum.TryParse<EstadoCompra>(value, true, out var parsed)
            ? parsed
            : EstadoCompra.Registrada;
    }
}
