using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class EfFacturacionRepository : IFacturacionRepository
{
    private static readonly HashSet<decimal> SupportedIvaRates = [0m, 5m, 8m, 15m];
    private readonly TestDeIaDbContext dbContext;

    public EfFacturacionRepository(TestDeIaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<PosClienteResponse>> SearchClientesAsync(string term, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term.Trim();

        if (normalizedTerm.Length < 2)
        {
            return Array.Empty<PosClienteResponse>();
        }

        var clientes = await dbContext.Clientes
            .AsNoTracking()
            .Include(cliente => cliente.Persona)
            .Where(cliente =>
                cliente.IsActive &&
                cliente.Persona.IsActive &&
                (cliente.Persona.Identificacion.Contains(normalizedTerm) ||
                 cliente.Persona.Nombres.Contains(normalizedTerm) ||
                 cliente.Persona.Apellidos.Contains(normalizedTerm)))
            .OrderBy(cliente => cliente.Persona.Identificacion)
            .Take(20)
            .ToListAsync(cancellationToken);

        return clientes.Select(cliente => new PosClienteResponse
        {
            ClienteId = cliente.Id,
            PersonaId = cliente.PersonaId,
            TipoIdentificacion = cliente.Persona.TipoIdentificacion,
            Identificacion = cliente.Persona.Identificacion,
            NombreCompleto = $"{cliente.Persona.Nombres} {cliente.Persona.Apellidos}".Trim(),
            Email = cliente.Persona.Email,
            Telefono = cliente.Persona.Telefono,
            Direccion = cliente.Persona.Direccion
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<PosProductoResponse>> SearchProductosAsync(string term, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term.Trim();

        if (normalizedTerm.Length < 2)
        {
            return Array.Empty<PosProductoResponse>();
        }

        var productos = await dbContext.Productos
            .AsNoTracking()
            .Where(producto =>
                producto.IsActive &&
                (producto.Codigo.Contains(normalizedTerm) ||
                 producto.Nombre.Contains(normalizedTerm)))
            .OrderBy(producto => producto.Nombre)
            .Take(20)
            .ToListAsync(cancellationToken);

        return productos.Select(producto => new PosProductoResponse
        {
            ProductoId = producto.Id,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            CodigoIva = producto.CodigoIva,
            PorcentajeIva = producto.PorcentajeIva,
            PrecioVenta = producto.PrecioVenta,
            StockActual = producto.StockActual
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

        var empresa = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("No existe una empresa emisora configurada para facturacion.");

        var itemsByProduct = request.Items
            .GroupBy(item => item.ProductoId)
            .Select(group => new { ProductoId = group.Key, Cantidad = group.Sum(item => item.Cantidad) })
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
        var factura = new FacturaEntity
        {
            Id = Guid.NewGuid(),
            EmpresaEmisoraId = empresa.Id,
            Establecimiento = empresa.Establecimiento,
            PuntoEmision = empresa.PuntoEmision,
            RucEmisor = empresa.Ruc,
            RazonSocialEmisor = empresa.RazonSocial,
            NombreComercialEmisor = empresa.NombreComercial,
            DireccionMatrizEmisor = empresa.DireccionMatriz,
            DireccionEstablecimientoEmisor = empresa.DireccionEstablecimiento,
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
            Estado = "Pendiente",
            Observacion = NormalizeOptional(request.Observacion),
            FechaEmision = now,
            CreatedAt = now,
            UpdatedAt = now,
            MensajeEstado = "Factura registrada localmente y en cola para procesamiento."
        };

        foreach (var item in itemsByProduct)
        {
            var producto = productos.First(current => current.Id == item.ProductoId);

            if (!SupportedIvaRates.Contains(producto.PorcentajeIva))
            {
                throw new InvalidOperationException($"El producto {producto.Nombre} tiene una tarifa de IVA no soportada.");
            }

            if (producto.StockActual < item.Cantidad)
            {
                throw new InvalidOperationException($"No hay stock suficiente para {producto.Nombre}.");
            }

            var subtotal = Math.Round(item.Cantidad * producto.PrecioVenta, 2);
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
                Subtotal = subtotal,
                IvaValor = ivaValor,
                Total = total
            });

            factura.Subtotal += subtotal;
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
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
            Mensaje = factura.MensajeEstado ?? "Factura registrada.",
            CreatedAt = now
        });

        dbContext.Set<FacturaEntity>().Add(factura);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new FacturaEmissionResponse
        {
            FacturaId = factura.Id,
            Secuencial = factura.Secuencial,
            Estado = factura.Estado,
            NumeroComprobante = $"{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}",
            Mensaje = "Factura registrada correctamente. El procesamiento SRI continua en segundo plano."
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
                factura.Estado == "Pendiente" ||
                (factura.Estado == "Error" && (!factura.NextRetryAt.HasValue || factura.NextRetryAt <= now)) ||
                (factura.Estado == "EnProceso" && factura.ProcessingStartedAt < staleProcessingLimit))
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
                (factura.Estado == "Pendiente" ||
                 (factura.Estado == "Error" && (!factura.NextRetryAt.HasValue || factura.NextRetryAt <= now)) ||
                 (factura.Estado == "EnProceso" && factura.ProcessingStartedAt < staleProcessingLimit)))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(factura => factura.Estado, "EnProceso")
                .SetProperty(factura => factura.ProcessingNode, workerId)
                .SetProperty(factura => factura.ProcessingStartedAt, now)
                .SetProperty(factura => factura.UpdatedAt, now)
                .SetProperty(factura => factura.RetryCount, factura => factura.RetryCount + 1)
                .SetProperty(factura => factura.MensajeEstado, "Factura tomada por un worker para envio al SRI."),
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

        if (factura is null || factura.Estado == "Autorizado" || factura.Estado == "Rechazado")
        {
            return;
        }

        factura.Estado = "Recibido";
        factura.MensajeEstado = mensaje;
        factura.UpdatedAt = DateTimeOffset.UtcNow;
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
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
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var factura = await dbContext.Set<FacturaEntity>()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken);

        if (factura is null || factura.Estado == "Autorizado")
        {
            return;
        }

        foreach (var detalle in factura.Detalles)
        {
            var producto = await dbContext.Productos
                .FirstOrDefaultAsync(current => current.Id == detalle.ProductoId, cancellationToken)
                ?? throw new InvalidOperationException("No se encontro un producto de la factura al intentar descargar inventario.");

            await RegistrarSalidaPorFacturaAsync(
                producto,
                $"{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}",
                detalle.Cantidad,
                cancellationToken);
        }

        factura.Estado = "Autorizado";
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
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task MarkFacturaAsRejectedAsync(
        Guid facturaId,
        string mensaje,
        string xmlFirmado,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default)
    {
        var factura = await dbContext.Set<FacturaEntity>()
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken);

        if (factura is null)
        {
            return;
        }

        factura.Estado = "Rechazado";
        factura.XmlFirmado = xmlFirmado;
        factura.MensajeEstado = mensaje;
        factura.UpdatedAt = fechaRespuesta;
        factura.ProcessingNode = null;
        factura.ProcessingStartedAt = null;
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

        factura.Estado = "Error";
        factura.MensajeEstado = mensaje;
        factura.UpdatedAt = DateTimeOffset.UtcNow;
        factura.ProcessingNode = null;
        factura.ProcessingStartedAt = null;
        factura.NextRetryAt = nextRetryAt;
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
            Mensaje = mensaje,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RegistrarSalidaPorFacturaAsync(
        ProductoEntity producto,
        string referenciaFactura,
        decimal cantidad,
        CancellationToken cancellationToken)
    {
        if (producto.StockActual < cantidad)
        {
            throw new InvalidOperationException($"No existe stock suficiente para {producto.Nombre} durante la autorizacion.");
        }

        producto.StockActual -= cantidad;
        producto.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.KardexMovimientos.Add(new KardexMovimientoEntity
        {
            Id = Guid.NewGuid(),
            ProductoId = producto.Id,
            TipoMovimiento = "Salida",
            Concepto = "Factura autorizada",
            Referencia = referenciaFactura,
            CantidadEntrada = 0,
            CantidadSalida = cantidad,
            SaldoCantidad = producto.StockActual,
            CostoUnitario = producto.CostoPromedio,
            CostoPromedio = producto.CostoPromedio,
            SaldoValor = producto.StockActual * producto.CostoPromedio,
            FechaMovimiento = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Factura MapDomain(FacturaEntity entity)
    {
        return new Factura(
            entity.Id,
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
            Estado = entity.Estado,
            Subtotal = entity.Subtotal,
            IvaTotal = entity.IvaTotal,
            Total = entity.Total,
            ClaveAcceso = entity.ClaveAcceso,
            NumeroAutorizacion = entity.NumeroAutorizacion,
            MensajeEstado = entity.MensajeEstado,
            FechaEmision = entity.FechaEmision,
            FechaAutorizacion = entity.FechaAutorizacion
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
}
