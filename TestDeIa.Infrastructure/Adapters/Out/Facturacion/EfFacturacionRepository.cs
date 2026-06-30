using Microsoft.EntityFrameworkCore;
using System.Data;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class EfFacturacionRepository : IFacturacionRepository
{
    private static readonly HashSet<decimal> SupportedIvaRates = [0m, 5m, 8m, 15m];
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;
    private readonly IInventarioRepository inventarioRepository;

    public EfFacturacionRepository(
        TestDeIaDbContext dbContext,
        ITenantContextAccessor tenantContextAccessor,
        IInventarioRepository inventarioRepository)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
        this.inventarioRepository = inventarioRepository;
    }

    public async Task<PagedResultResponse<PosClienteResponse>> SearchClientesAsync(string term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term.Trim();

        if (normalizedTerm.Length < 2)
        {
            return new PagedResultResponse<PosClienteResponse> { Items = Array.Empty<PosClienteResponse>(), Skip = skip, Take = take };
        }

        var query = dbContext.Clientes
            .AsNoTracking()
            .Include(cliente => cliente.Persona)
            .Where(cliente =>
                cliente.IsActive &&
                cliente.Persona.IsActive &&
                (cliente.Persona.Identificacion.Contains(normalizedTerm) ||
                 cliente.Persona.Nombres.Contains(normalizedTerm) ||
                 cliente.Persona.Apellidos.Contains(normalizedTerm)));
        var totalCount = await query.CountAsync(cancellationToken);
        var clientes = await query
            .OrderBy(cliente => cliente.Persona.Identificacion)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<PosClienteResponse>
        {
            Items = clientes.Select(cliente => new PosClienteResponse
            {
                ClienteId = cliente.Id,
                PersonaId = cliente.PersonaId,
                TipoIdentificacion = cliente.Persona.TipoIdentificacion,
                Identificacion = cliente.Persona.Identificacion,
                NombreCompleto = $"{cliente.Persona.Nombres} {cliente.Persona.Apellidos}".Trim(),
                Email = cliente.Persona.Email,
                Telefono = cliente.Persona.Telefono,
                Direccion = cliente.Persona.Direccion
            }).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<PagedResultResponse<PosProductoResponse>> SearchProductosAsync(string term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term.Trim();

        if (normalizedTerm.Length < 2)
        {
            return new PagedResultResponse<PosProductoResponse> { Items = Array.Empty<PosProductoResponse>(), Skip = skip, Take = take };
        }

        var query = dbContext.Productos
            .AsNoTracking()
            .Where(producto =>
                producto.IsActive &&
                (producto.Codigo.Contains(normalizedTerm) ||
                 producto.Nombre.Contains(normalizedTerm)))
            ;

        var totalCount = await query.CountAsync(cancellationToken);
        var productos = await query
            .OrderBy(producto => producto.Nombre)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<PosProductoResponse>
        {
            Items = productos.Select(producto => new PosProductoResponse
            {
                ProductoId = producto.Id,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                CodigoIva = producto.CodigoIva,
                PorcentajeIva = producto.PorcentajeIva,
                PrecioVenta = producto.PrecioVenta,
                StockActual = producto.StockActual,
                ControlaStock = producto.ControlaStock
            }).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<IReadOnlyCollection<PosPuntoEmisionResponse>> GetPuntosEmisionAsync(CancellationToken cancellationToken = default)
    {
        var empresaActivaId = tenantContextAccessor.EmpresaId;
        if (!empresaActivaId.HasValue)
        {
            return Array.Empty<PosPuntoEmisionResponse>();
        }

        var puntos = await dbContext.EmpresaPuntosEmision
            .AsNoTracking()
            .Where(current => current.EmpresaEmisoraId == empresaActivaId.Value)
            .OrderByDescending(current => current.IsDefault)
            .ThenBy(current => current.Establecimiento)
            .ThenBy(current => current.PuntoEmision)
            .ToListAsync(cancellationToken);

        return puntos.Select(current => new PosPuntoEmisionResponse
        {
            Establecimiento = current.Establecimiento,
            PuntoEmision = current.PuntoEmision,
            DireccionEstablecimiento = current.DireccionEstablecimiento,
            IsDefault = current.IsDefault
        }).ToArray();
    }

    public async Task<FacturaEmissionResponse> CreatePendingFacturaAsync(
        EmitirFacturaRequest request,
        CancellationToken cancellationToken = default)
    {
        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .Include(current => current.Persona)
            .FirstOrDefaultAsync(current => current.Id == request.ClienteId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro el cliente seleccionado.");

        var empresaActivaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para la factura.");

        var empresa = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == empresaActivaId && current.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("No existe una empresa emisora configurada para facturacion.");

        var puntoEmision = await dbContext.EmpresaPuntosEmision
            .AsNoTracking()
            .FirstOrDefaultAsync(current =>
                current.EmpresaEmisoraId == empresa.Id &&
                current.Establecimiento == request.Establecimiento.Trim() &&
                current.PuntoEmision == request.PuntoEmision.Trim(),
                cancellationToken)
            ?? throw new InvalidOperationException("El establecimiento y punto de emision seleccionados no pertenecen a la empresa activa.");

        var itemsByProduct = request.Items
            .GroupBy(item => item.ProductoId)
            .Select(group => new
            {
                ProductoId = group.Key,
                Cantidad = group.Sum(item => item.Cantidad),
                Descuento = group.Sum(item => item.Descuento)
            })
            .ToArray();

        var productIds = itemsByProduct.Select(item => item.ProductoId).ToArray();
        var productos = await dbContext.Productos
            .Where(producto => productIds.Contains(producto.Id) && producto.IsActive)
            .ToListAsync(cancellationToken);

        if (productos.Count != productIds.Length)
        {
            throw new InvalidOperationException("Uno o varios productos ya no estan disponibles.");
        }

        var now = DateTimeOffset.UtcNow;
        var clienteTipoIdentificacion = MapClienteTipoIdentificacionSri(cliente.Persona.TipoIdentificacion);
        var formaPago = request.FormaPago.Trim();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var secuencial = await ReserveNextSecuencialAsync(
            empresa.Id,
            puntoEmision.Establecimiento,
            puntoEmision.PuntoEmision,
            now,
            cancellationToken);

        var factura = new FacturaEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpresaEmisoraId = empresa.Id,
            Secuencial = secuencial,
            Establecimiento = puntoEmision.Establecimiento,
            PuntoEmision = puntoEmision.PuntoEmision,
            RucEmisor = empresa.Ruc,
            RazonSocialEmisor = empresa.RazonSocial,
            NombreComercialEmisor = empresa.NombreComercial,
            DireccionMatrizEmisor = empresa.DireccionMatriz,
            DireccionEstablecimientoEmisor = puntoEmision.DireccionEstablecimiento ?? empresa.DireccionEstablecimiento,
            AmbienteSri = empresa.AmbienteSri,
            TipoEmision = empresa.TipoEmision,
            ObligadoContabilidad = empresa.ObligadoContabilidad,
            ContribuyenteEspecial = empresa.ContribuyenteEspecial,
            RegimenRimpe = empresa.RegimenRimpe,
            AgenteRetencionResolucion = empresa.AgenteRetencionResolucion,
            ClienteId = cliente.Id,
            ClienteTipoIdentificacion = clienteTipoIdentificacion,
            ClienteIdentificacion = cliente.Persona.Identificacion,
            ClienteNombre = $"{cliente.Persona.Nombres} {cliente.Persona.Apellidos}".Trim(),
            ClienteDireccion = NormalizeOptional(cliente.Persona.Direccion),
            ClienteEmail = NormalizeOptional(cliente.Persona.Email),
            ClienteTelefono = NormalizeOptional(cliente.Persona.Telefono),
            FormaPago = formaPago,
            FormaPagoSriCodigo = MapFormaPagoSriCodigo(formaPago),
            Estado = FacturaEstado.NO_FIRMADO,
            Observacion = NormalizeOptional(request.Observacion),
            FechaEmision = now,
            CreatedAt = now,
            UpdatedAt = now,
            ClaveAcceso = string.Empty,
            MensajeEstado = "Factura registrada localmente. Se preparara el XML electronico."
        };

        foreach (var item in itemsByProduct)
        {
            var producto = productos.First(current => current.Id == item.ProductoId);

            if (!SupportedIvaRates.Contains(producto.PorcentajeIva))
            {
                throw new InvalidOperationException($"El producto {producto.Nombre} tiene una tarifa de IVA no soportada.");
            }

            if (producto.ControlaStock && producto.StockActual < item.Cantidad)
            {
                throw new InvalidOperationException($"No hay stock suficiente para {producto.Nombre}.");
            }

            var subtotalBruto = Math.Round(item.Cantidad * producto.PrecioVenta, 2);
            var descuento = Math.Round(item.Descuento, 2);
            if (descuento > subtotalBruto)
            {
                throw new InvalidOperationException($"El descuento configurado para {producto.Nombre} no puede superar el subtotal de la linea.");
            }

            var subtotal = subtotalBruto - descuento;
            var ivaValor = Math.Round(subtotal * (producto.PorcentajeIva / 100m), 2);
            var total = subtotal + ivaValor;

            factura.Detalles.Add(new FacturaDetalleEntity
            {
                Id = Guid.NewGuid(),
                FacturaId = factura.Id,
                ProductoId = producto.Id,
                CodigoProducto = producto.Codigo,
                NombreProducto = producto.Nombre,
                CodigoIva = producto.CodigoIva,
                PorcentajeIva = producto.PorcentajeIva,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.PrecioVenta,
                Descuento = descuento,
                Subtotal = subtotal,
                IvaValor = ivaValor,
                Total = total
            });

            factura.Subtotal += subtotal;
            factura.TotalDescuento += descuento;
            factura.IvaTotal += ivaValor;

            switch (producto.PorcentajeIva)
            {
                case 0m:
                    factura.SubtotalIva0 += subtotal;
                    break;
                case 5m:
                    factura.SubtotalIva5 += subtotal;
                    break;
                case 8m:
                    factura.SubtotalIva8 += subtotal;
                    break;
                case 15m:
                    factura.SubtotalIva15 += subtotal;
                    break;
            }
        }

        factura.Total = factura.Subtotal + factura.IvaTotal;
        dbContext.Set<FacturaEntity>().Add(factura);
        await dbContext.SaveChangesAsync(cancellationToken);
        dbContext.ChangeTracker.Clear();

        var persistedFactura = await dbContext.Set<FacturaEntity>()
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstAsync(current => current.Id == factura.Id, cancellationToken);

        var domainFactura = MapDomain(persistedFactura);
        var claveAcceso = SriFacturaXmlBuilder.GenerateClaveAcceso(domainFactura);
        var xmlGenerado = SriFacturaXmlBuilder.BuildUnsignedXml(domainFactura, claveAcceso);
        var updatedAt = DateTimeOffset.UtcNow;
        const FacturaEstado estadoFinal = FacturaEstado.NO_FIRMADO;
        const string mensajeEstado = "XML generado correctamente. El comprobante fue enviado a la cola de firma electronica.";

        await dbContext.Set<FacturaEntity>()
            .Where(current => current.Id == factura.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(current => current.ClaveAcceso, claveAcceso)
                .SetProperty(current => current.XmlGenerado, xmlGenerado)
                .SetProperty(current => current.XmlFirmado, (string?)null)
                .SetProperty(current => current.Estado, estadoFinal)
                .SetProperty(current => current.MensajeEstado, mensajeEstado)
                .SetProperty(current => current.UpdatedAt, updatedAt),
                cancellationToken);

        dbContext.FacturaSriEventos.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = estadoFinal,
            Mensaje = mensajeEstado,
            CreatedAt = updatedAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new FacturaEmissionResponse
        {
            FacturaId = persistedFactura.Id,
            Secuencial = persistedFactura.Secuencial,
            Estado = estadoFinal.ToApiValue(),
            NumeroComprobante = $"{persistedFactura.Establecimiento}-{persistedFactura.PuntoEmision}-{persistedFactura.Secuencial:000000000}",
            Mensaje = "Factura registrada localmente y enviada a la cola de firma electronica."
        };
    }

    public async Task<IReadOnlyCollection<FacturaMonitorResponse>> GetMonitorAsync(CancellationToken cancellationToken = default)
    {
        var facturas = await dbContext.Set<FacturaEntity>()
            .AsNoTracking()
            .OrderByDescending(factura => factura.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        return facturas.Select(MapMonitor).ToArray();
    }

    public async Task<IReadOnlyCollection<Guid>> GetPendingFacturaIdsAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var staleProcessingLimit = now.AddMinutes(-2);

        return await dbContext.Set<FacturaEntity>()
            .AsNoTracking()
            .Where(factura =>
                factura.Estado == FacturaEstado.NO_FIRMADO &&
                factura.ClaveAcceso != string.Empty &&
                factura.XmlGenerado != null &&
                factura.XmlFirmado == null &&
                factura.NumeroAutorizacion == null &&
                factura.RetryCount == 0 &&
                (!factura.NextRetryAt.HasValue || factura.NextRetryAt <= now || factura.ProcessingStartedAt < staleProcessingLimit))
            .OrderBy(factura => factura.CreatedAt)
            .Take(batchSize)
            .Select(factura => factura.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Factura?> TryClaimFacturaAsync(Guid facturaId, string workerId, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var staleProcessingLimit = now.AddMinutes(-2);

        var affected = await dbContext.Set<FacturaEntity>()
            .Where(factura =>
                factura.Id == facturaId &&
                factura.Estado == FacturaEstado.NO_FIRMADO &&
                factura.ClaveAcceso != string.Empty &&
                factura.XmlGenerado != null &&
                factura.XmlFirmado == null &&
                factura.NumeroAutorizacion == null &&
                factura.RetryCount == 0 &&
                (!factura.NextRetryAt.HasValue || factura.NextRetryAt <= now || factura.ProcessingStartedAt < staleProcessingLimit))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(factura => factura.ProcessingNode, workerId)
                .SetProperty(factura => factura.ProcessingStartedAt, now)
                .SetProperty(factura => factura.UpdatedAt, now)
                .SetProperty(factura => factura.RetryCount, factura => factura.RetryCount + 1)
                .SetProperty(factura => factura.MensajeEstado, "Factura tomada por un worker para firma electronica."),
                cancellationToken);

        if (affected == 0)
        {
            return null;
        }

        var entity = await dbContext.Set<FacturaEntity>()
            .AsNoTracking()
            .Include(factura => factura.Detalles)
            .FirstAsync(factura => factura.Id == facturaId, cancellationToken);

        return MapDomain(entity);
    }

    public async Task MarkFacturaAsReceivedAsync(Guid facturaId, string mensaje, CancellationToken cancellationToken = default)
    {
        var factura = await dbContext.Set<FacturaEntity>()
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken);

        if (factura is null || factura.Estado.IsFinal())
        {
            return;
        }

        factura.MensajeEstado = mensaje;
        factura.UpdatedAt = DateTimeOffset.UtcNow;
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = FacturaEstado.PENDIENTE,
            Mensaje = mensaje,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFacturaAsAuthorizedAsync(
        Guid facturaId,
        string claveAcceso,
        string? numeroAutorizacion,
        string xmlFirmado,
        string mensaje,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default)
    {
        var factura = await dbContext.Set<FacturaEntity>()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken);

        if (factura is null || factura.Estado == FacturaEstado.AUTORIZADO)
        {
            return;
        }

        EnsureStableClaveAcceso(factura, claveAcceso);

        await ApplyInventoryIfNeededAsync(factura, "Factura autorizada", cancellationToken);

        factura.Estado = FacturaEstado.AUTORIZADO;
        factura.ClaveAcceso = claveAcceso;
        factura.NumeroAutorizacion = numeroAutorizacion;
        factura.XmlFirmado = xmlFirmado;
        factura.MensajeEstado = mensaje;
        factura.FechaAutorizacion = fechaRespuesta;
        factura.UpdatedAt = fechaRespuesta;
        factura.ProcessingNode = null;
        factura.ProcessingStartedAt = null;
        factura.NextRetryAt = null;
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
            Mensaje = mensaje,
            CreatedAt = fechaRespuesta
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFacturaAsSignedPendingAsync(
        Guid facturaId,
        string claveAcceso,
        string xmlFirmado,
        string mensaje,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default)
    {
        var factura = await dbContext.Set<FacturaEntity>()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken);

        if (factura is null)
        {
            return;
        }

        EnsureStableClaveAcceso(factura, claveAcceso);
        await ApplyInventoryIfNeededAsync(factura, "Factura pendiente", cancellationToken);

        factura.Estado = FacturaEstado.PENDIENTE;
        factura.ClaveAcceso = claveAcceso;
        factura.XmlFirmado = xmlFirmado;
        factura.MensajeEstado = mensaje;
        factura.UpdatedAt = fechaRespuesta;
        factura.ProcessingNode = null;
        factura.ProcessingStartedAt = null;
        factura.NextRetryAt = null;
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
            Mensaje = mensaje,
            CreatedAt = fechaRespuesta
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFacturaAsRejectedAsync(
        Guid facturaId,
        string mensaje,
        string? xmlFirmado,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default)
    {
        var factura = await dbContext.Set<FacturaEntity>()
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken);

        if (factura is null)
        {
            return;
        }

        factura.Estado = FacturaEstado.RECHAZADO;
        factura.XmlFirmado = xmlFirmado;
        factura.MensajeEstado = mensaje;
        factura.UpdatedAt = fechaRespuesta;
        factura.ProcessingNode = null;
        factura.ProcessingStartedAt = null;
        factura.NextRetryAt = null;
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
            Mensaje = mensaje,
            CreatedAt = fechaRespuesta
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFacturaAsUnsignedAsync(
        Guid facturaId,
        string claveAcceso,
        string mensaje,
        string xmlGenerado,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default)
    {
        var factura = await dbContext.Set<FacturaEntity>()
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken);

        if (factura is null)
        {
            return;
        }

        EnsureStableClaveAcceso(factura, claveAcceso);

        factura.Estado = FacturaEstado.NO_FIRMADO;
        factura.ClaveAcceso = claveAcceso;
        factura.XmlGenerado = xmlGenerado;
        factura.XmlFirmado = null;
        factura.MensajeEstado = mensaje;
        factura.UpdatedAt = fechaRespuesta;
        factura.ProcessingNode = null;
        factura.ProcessingStartedAt = null;
        factura.NextRetryAt = DateTimeOffset.MaxValue;
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
            Mensaje = mensaje,
            CreatedAt = fechaRespuesta
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFacturaAsErrorAsync(
        Guid facturaId,
        string mensaje,
        DateTimeOffset nextRetryAt,
        CancellationToken cancellationToken = default)
    {
        var factura = await dbContext.Set<FacturaEntity>()
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken);

        if (factura is null)
        {
            return;
        }

        factura.MensajeEstado = mensaje;
        factura.UpdatedAt = DateTimeOffset.UtcNow;
        factura.ProcessingNode = null;
        factura.ProcessingStartedAt = null;
        factura.NextRetryAt = nextRetryAt;
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = FacturaEstado.PENDIENTE,
            Mensaje = mensaje,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyInventoryIfNeededAsync(
        FacturaEntity factura,
        string concepto,
        CancellationToken cancellationToken)
    {
        if (factura.InventarioAplicado)
        {
            return;
        }

        await inventarioRepository.DescontarStockPorFacturaAsync(
            factura.Id,
            $"{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}",
            concepto,
            factura.Detalles.Select(detalle => (detalle.ProductoId, detalle.Cantidad)).ToArray(),
            cancellationToken);

        var inventoryAppliedAt = DateTimeOffset.UtcNow;

        await dbContext.Set<FacturaEntity>()
            .Where(current => current.Id == factura.Id && !current.InventarioAplicado)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(current => current.InventarioAplicado, true)
                .SetProperty(current => current.InventarioAplicadoAt, inventoryAppliedAt)
                .SetProperty(current => current.UpdatedAt, inventoryAppliedAt),
                cancellationToken);

        factura.InventarioAplicado = true;
        factura.InventarioAplicadoAt = inventoryAppliedAt;
        factura.UpdatedAt = inventoryAppliedAt;

        var inventoryEvent = new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
            Mensaje = $"Inventario transaccional aplicado. Referencia {factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}.",
            CreatedAt = inventoryAppliedAt
        };

        dbContext.FacturaSriEventos.Add(inventoryEvent);

        if (dbContext.Entry(factura).State != EntityState.Detached)
        {
            factura.EventosSri.Add(inventoryEvent);
        }
    }

    private static Factura MapDomain(FacturaEntity entity)
    {
        return new Factura(
            entity.Id,
            entity.EmpresaId,
            entity.Secuencial,
            entity.Establecimiento,
            entity.PuntoEmision,
            entity.RucEmisor,
            entity.RazonSocialEmisor,
            entity.NombreComercialEmisor,
            entity.DireccionMatrizEmisor,
            entity.DireccionEstablecimientoEmisor,
            entity.AmbienteSri,
            entity.TipoEmision,
            entity.ObligadoContabilidad,
            entity.ContribuyenteEspecial,
            entity.RegimenRimpe,
            entity.AgenteRetencionResolucion,
            entity.ClienteId,
            entity.ClienteTipoIdentificacion,
            entity.ClienteIdentificacion,
            entity.ClienteNombre,
            entity.ClienteDireccion,
            entity.ClienteEmail,
            entity.ClienteTelefono,
            entity.FormaPago,
            entity.FormaPagoSriCodigo,
            entity.Estado,
            entity.Subtotal,
            entity.TotalDescuento,
            entity.SubtotalIva0,
            entity.SubtotalIva5,
            entity.SubtotalIva8,
            entity.SubtotalIva15,
            entity.IvaTotal,
            entity.Total,
            entity.Observacion,
            entity.ClaveAcceso,
            entity.NumeroAutorizacion,
            entity.MensajeEstado,
            entity.XmlGenerado,
            entity.XmlFirmado,
            entity.FechaEmision,
            entity.FechaAutorizacion,
            entity.Detalles.Select(detalle => new FacturaDetalle(
                detalle.Id,
                detalle.FacturaId,
                detalle.ProductoId,
                detalle.CodigoProducto,
                detalle.NombreProducto,
                detalle.CodigoIva,
                detalle.PorcentajeIva,
                detalle.Cantidad,
                detalle.PrecioUnitario,
                detalle.Descuento,
                detalle.Subtotal,
                detalle.IvaValor,
                detalle.Total)).ToArray());
    }

    private static FacturaMonitorResponse MapMonitor(FacturaEntity entity)
    {
        return new FacturaMonitorResponse
        {
            Id = entity.Id,
            Secuencial = entity.Secuencial,
            Establecimiento = entity.Establecimiento,
            PuntoEmision = entity.PuntoEmision,
            ClienteIdentificacion = entity.ClienteIdentificacion,
            ClienteTipoIdentificacion = entity.ClienteTipoIdentificacion,
            ClienteNombre = entity.ClienteNombre,
            FormaPago = entity.FormaPago,
            Estado = entity.Estado.ToApiValue(),
            Subtotal = entity.Subtotal,
            IvaTotal = entity.IvaTotal,
            Total = entity.Total,
            ClaveAcceso = entity.ClaveAcceso,
            NumeroAutorizacion = entity.NumeroAutorizacion,
            MensajeEstado = entity.MensajeEstado,
            TieneXmlGenerado = !string.IsNullOrWhiteSpace(entity.XmlGenerado),
            TieneXmlFirmado = !string.IsNullOrWhiteSpace(entity.XmlFirmado),
            FechaEmision = entity.FechaEmision,
            FechaAutorizacion = entity.FechaAutorizacion
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void EnsureStableClaveAcceso(FacturaEntity factura, string claveAcceso)
    {
        if (!string.IsNullOrWhiteSpace(factura.ClaveAcceso) &&
            !string.Equals(factura.ClaveAcceso, claveAcceso, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"La factura {factura.Id} ya fue registrada con la clave de acceso {factura.ClaveAcceso} y no puede continuar con una clave distinta ({claveAcceso}).");
        }
    }

    private static string MapClienteTipoIdentificacionSri(string? tipoIdentificacion)
    {
        var normalized = NormalizeOptional(tipoIdentificacion)?.ToUpperInvariant();

        return normalized switch
        {
            "RUC" => "04",
            "CEDULA" => "05",
            "CÉDULA" => "05",
            "PASAPORTE" => "06",
            "CONSUMIDOR FINAL" => "07",
            "IDENTIFICACION DEL EXTERIOR" => "08",
            "IDENTIFICACIÓN DEL EXTERIOR" => "08",
            "PLACA" => "09",
            _ => throw new InvalidOperationException("El cliente no tiene un tipo de identificacion compatible con la ficha tecnica del SRI.")
        };
    }

    private static string MapFormaPagoSriCodigo(string formaPago)
    {
        var normalized = formaPago.Trim().ToUpperInvariant();

        return normalized switch
        {
            "EFECTIVO" => "01",
            "COMPENSACION" => "15",
            "COMPENSACIÓN" => "15",
            "TARJETA DE DEBITO" => "16",
            "TARJETA DE DÉBITO" => "16",
            "DINERO ELECTRONICO" => "17",
            "DINERO ELECTRÓNICO" => "17",
            "TARJETA PREPAGO" => "18",
            "TARJETA" => "19",
            "TARJETA DE CREDITO" => "19",
            "TARJETA DE CRÉDITO" => "19",
            "TRANSFERENCIA" => "20",
            "OTROS CON UTILIZACION DEL SISTEMA FINANCIERO" => "20",
            "OTROS CON UTILIZACIÓN DEL SISTEMA FINANCIERO" => "20",
            "ENDOSO DE TITULOS" => "21",
            "ENDOSO DE TÍTULOS" => "21",
            _ => throw new InvalidOperationException("La forma de pago seleccionada no esta mapeada a un codigo SRI valido.")
        };
    }

    private async Task<long> ReserveNextSecuencialAsync(
        Guid empresaId,
        string establecimiento,
        string puntoEmision,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var record = await dbContext.FacturaSecuenciales
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.Establecimiento == establecimiento &&
                current.PuntoEmision == puntoEmision,
                cancellationToken);

        if (record is null)
        {
            record = new FacturaSecuencialEntity
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
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
        return record.UltimoSecuencial;
    }
}
