using Microsoft.EntityFrameworkCore;
using System.Data;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Domain.Modules.Caja.Entities;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Facturacion;
using TestDeIa.Shared.Security;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class EfFacturacionRepository : IFacturacionRepository
{
    private static readonly HashSet<decimal> SupportedIvaRates = [0m, 5m, 8m, 15m];
    private const string PrincipalBodegaName = "Principal";
    private const int MaxRetryDelayMinutes = 15;
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

        var query = dbContext.Personas
            .AsNoTracking()
            .Include(persona => persona.Cliente)
            .Where(persona =>
                !persona.IsSystemRecord &&
                persona.IsActive &&
                (persona.Identificacion.Contains(normalizedTerm) ||
                 persona.RazonSocialONombresCompletos.Contains(normalizedTerm) ||
                 (persona.NombreComercial != null && persona.NombreComercial.Contains(normalizedTerm))));
        var totalCount = await query.CountAsync(cancellationToken);
        var personas = await query
            .OrderBy(persona => persona.Identificacion)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<PosClienteResponse>
        {
            Items = personas.Select(persona => new PosClienteResponse
            {
                ClienteId = persona.Cliente?.PersonaId,
                PersonaId = persona.Id,
                TipoIdentificacion = persona.TipoIdentificacion,
                Identificacion = persona.Identificacion,
                NombreCompleto = persona.RazonSocialONombresCompletos,
                NombreComercial = persona.NombreComercial,
                Email = persona.CorreoElectronicoPrincipal,
                Telefono = persona.TelefonoCelular,
                Direccion = persona.DireccionPrincipal,
                HasClienteExtension = persona.Cliente is not null && persona.Cliente.IsActive
            }).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<PagedResultResponse<PosProductoResponse>> SearchProductosAsync(string term, int skip, int take, Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term.Trim();

        if (normalizedTerm.Length < 2)
        {
            return new PagedResultResponse<PosProductoResponse> { Items = Array.Empty<PosProductoResponse>(), Skip = skip, Take = take };
        }

        var operationalBodegaId = await ResolveOperationalBodegaIdAsync(bodegaId, cancellationToken);

        var query = dbContext.Productos
            .AsNoTracking()
            .Include(producto => producto.ProductosBodega)
            .Where(producto =>
                producto.IsActive &&
                (!producto.ControlaStock || producto.ProductosBodega.Any(existencia => existencia.BodegaId == operationalBodegaId)) &&
                (producto.Codigo.Contains(normalizedTerm) ||
                 producto.Nombre.Contains(normalizedTerm) ||
                 (producto.Descripcion != null && producto.Descripcion.Contains(normalizedTerm))));

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
                StockActual = producto.ProductosBodega
                    .Where(current => current.BodegaId == operationalBodegaId)
                    .Select(current => current.StockActual)
                    .FirstOrDefault(),
                ControlaStock = producto.ControlaStock,
                AplicaComision = producto.AplicaComision,
                TipoComision = producto.TipoComision,
                ValorComision = producto.ValorComision
            }).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }


    public async Task<IReadOnlyCollection<PosOperadorResponse>> GetOperadoresAsync(CancellationToken cancellationToken = default)
    {
        var empresaActivaId = tenantContextAccessor.EmpresaId;
        if (!empresaActivaId.HasValue)
        {
            return Array.Empty<PosOperadorResponse>();
        }

        return await dbContext.SecurityUsers
            .AsNoTracking()
            .Include(usuario => usuario.Persona)
            .Include(usuario => usuario.EmpresasAcceso)
            .Where(usuario =>
                usuario.IsActive &&
                !usuario.BloqueadoManualmente &&
                (usuario.BloqueadoHasta == null || usuario.BloqueadoHasta <= DateTimeOffset.UtcNow) &&
                (usuario.EmpresaId == empresaActivaId.Value || usuario.EmpresasAcceso.Any(acceso => acceso.EmpresaId == empresaActivaId.Value)))
            .OrderBy(usuario => usuario.DisplayName)
            .Select(usuario => new PosOperadorResponse
            {
                UsuarioId = usuario.Id,
                UserName = usuario.UserName,
                NombreCompleto = string.IsNullOrWhiteSpace(usuario.DisplayName) ? usuario.Persona.RazonSocialONombresCompletos : usuario.DisplayName,
                Identificacion = usuario.Persona.Identificacion
            })
            .ToArrayAsync(cancellationToken);
    }
    public async Task<IReadOnlyCollection<PosPuntoEmisionResponse>> GetPuntosEmisionAsync(CancellationToken cancellationToken = default)
    {
        var empresaActivaId = tenantContextAccessor.EmpresaId;
        if (!empresaActivaId.HasValue)
        {
            return Array.Empty<PosPuntoEmisionResponse>();
        }

        var usuarioId = tenantContextAccessor.UserId;
        var roles = await GetCurrentUserRolesAsync(cancellationToken);
        var isAdmin = roles.Contains(SecurityRoleNames.Administrador, StringComparer.OrdinalIgnoreCase);
        var isCajero = roles.Contains(SecurityRoleNames.Cajero, StringComparer.OrdinalIgnoreCase);

        var query = dbContext.EmpresaPuntosEmision
            .AsNoTracking()
            .Include(current => current.Bodega)
            .Where(current => current.EmpresaEmisoraId == empresaActivaId.Value);

        if (!isAdmin && isCajero && usuarioId.HasValue)
        {
            var puntosPermitidos = dbContext.Set<SecurityUserPuntoEmisionEntity>()
                .AsNoTracking()
                .Where(current => current.SecurityUserId == usuarioId.Value && current.EmpresaId == empresaActivaId.Value)
                .Select(current => current.EmpresaPuntoEmisionId);

            query = query.Where(current => puntosPermitidos.Contains(current.Id));
        }

        var puntos = await query
            .OrderByDescending(current => current.IsDefault)
            .ThenBy(current => current.Establecimiento)
            .ThenBy(current => current.PuntoEmision)
            .ToListAsync(cancellationToken);

        return puntos.Select(current => new PosPuntoEmisionResponse
        {
            BodegaId = current.BodegaId,
            BodegaNombre = current.Bodega.Nombre,
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
            .FirstOrDefaultAsync(current => current.PersonaId == request.ClienteId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro el cliente seleccionado.");

        var empresaActivaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para la factura.");

        var empresa = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == empresaActivaId && current.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("No existe una empresa emisora configurada para facturacion.");

        var usuarioId = tenantContextAccessor.UserId ?? throw new InvalidOperationException("No se pudo identificar al cajero autenticado.");
        var cajaActiva = await dbContext.Set<CajaSesionEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaActivaId &&
                current.UsuarioId == usuarioId &&
                current.EstadoCaja == CajaEstado.Abierta,
                cancellationToken)
            ?? throw new InvalidOperationException("Debes abrir una caja antes de facturar en el POS.");

        var puntoEmision = await dbContext.EmpresaPuntosEmision
            .AsNoTracking()
            .Include(current => current.Bodega)
            .FirstOrDefaultAsync(current =>
                current.EmpresaEmisoraId == empresa.Id &&
                current.Establecimiento == request.Establecimiento.Trim() &&
                current.PuntoEmision == request.PuntoEmision.Trim(),
                cancellationToken)
            ?? throw new InvalidOperationException("El establecimiento y punto de emision seleccionados no pertenecen a la empresa activa.");

        await EnsurePuntoEmisionAllowedForCurrentUserAsync(usuarioId, empresaActivaId, puntoEmision.Id, cancellationToken);

        if (!puntoEmision.Bodega.IsActive)
        {
            throw new InvalidOperationException($"La bodega {puntoEmision.Bodega.Nombre} asociada al punto de emision no se encuentra activa.");
        }

        if (request.BodegaId.HasValue &&
            request.BodegaId.Value != Guid.Empty &&
            request.BodegaId.Value != puntoEmision.BodegaId)
        {
            throw new InvalidOperationException("El POS intento facturar con una bodega distinta a la asignada al punto de emision activo.");
        }

        var itemsByProduct = request.Items
            .GroupBy(item => new { item.ProductoId, item.UsuarioIdOperador })
            .Select(group => new
            {
                group.Key.ProductoId,
                group.Key.UsuarioIdOperador,
                Cantidad = group.Sum(item => item.Cantidad),
                Descuento = group.Sum(item => item.Descuento),
                PrecioUnitarioOverride = group
                    .Select(item => item.PrecioUnitarioOverride)
                    .LastOrDefault(value => value.HasValue)
            })
            .ToArray();

        var productIds = itemsByProduct.Select(item => item.ProductoId).ToArray();
        var operationalBodegaId = puntoEmision.BodegaId;
        var productos = await dbContext.Productos
            .Include(producto => producto.ProductosBodega)
            .Where(producto => productIds.Contains(producto.Id) && producto.IsActive)
            .ToListAsync(cancellationToken);

        if (productos.Count != productIds.Length)
        {
            throw new InvalidOperationException("Uno o varios productos ya no estan disponibles.");
        }

        var serviceItems = itemsByProduct
            .Where(item => !productos.First(producto => producto.Id == item.ProductoId).ControlaStock)
            .ToArray();

        if (serviceItems.Any(item => !item.UsuarioIdOperador.HasValue || item.UsuarioIdOperador.Value == Guid.Empty))
        {
            throw new InvalidOperationException("Todos los servicios deben registrar el operador que realizo el trabajo.");
        }

        var operadorIds = serviceItems
            .Select(item => item.UsuarioIdOperador!.Value)
            .Distinct()
            .ToArray();

        await EnsureOperadoresAllowedForActiveEmpresaAsync(operadorIds, empresaActivaId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var clienteTipoIdentificacion = MapClienteTipoIdentificacionSri(cliente.Persona.TipoIdentificacion);
        var formaPagoCodigo = MapFormaPagoSriCodigo(request.FormaPago);
        var formaPago = SriCatalogCodes.GetFormaPagoName(formaPagoCodigo);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var secuencial = await ReserveNextSecuencialAsync(
            empresa.Id,
            "01",
            puntoEmision.Establecimiento,
            puntoEmision.PuntoEmision,
            now,
            cancellationToken);

        var factura = new FacturaEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpresaEmisoraId = empresa.Id,
            BodegaId = operationalBodegaId,
            UsuarioId = usuarioId,
            CajaSesionId = cajaActiva.Id,
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
            ClienteId = cliente.PersonaId,
            ClienteTipoIdentificacion = clienteTipoIdentificacion,
            ClienteIdentificacion = cliente.Persona.Identificacion,
            ClienteNombre = cliente.Persona.RazonSocialONombresCompletos,
            ClienteDireccion = NormalizeOptional(cliente.Persona.DireccionPrincipal),
            ClienteEmail = NormalizeOptional(cliente.Persona.CorreoElectronicoPrincipal),
            ClienteTelefono = NormalizeOptional(cliente.Persona.TelefonoCelular),
            FormaPago = formaPago,
            FormaPagoSriCodigo = formaPagoCodigo,
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

            var stockDespacho = producto.ProductosBodega
                .Where(current => current.BodegaId == operationalBodegaId)
                .Select(current => current.StockActual)
                .FirstOrDefault();

            if (producto.ControlaStock && stockDespacho < item.Cantidad)
            {
                var disponibilidadAlterna = await BuildDisponibilidadAlternaMessageAsync(
                    producto.Id,
                    operationalBodegaId,
                    item.Cantidad - stockDespacho,
                    cancellationToken);

                throw new InvalidOperationException($"No hay stock suficiente para {producto.Nombre} en la bodega {puntoEmision.Bodega.Nombre}. Stock local: {stockDespacho:0.####}. Requerido: {item.Cantidad:0.####}.{disponibilidadAlterna}");
            }

            var precioUnitario = producto.ControlaStock ? producto.PrecioVenta : item.PrecioUnitarioOverride ?? producto.PrecioVenta;
            var subtotalBruto = Math.Round(item.Cantidad * precioUnitario, 2);
            var descuento = Math.Round(item.Descuento, 2);
            if (descuento > subtotalBruto)
            {
                throw new InvalidOperationException($"El descuento configurado para {producto.Nombre} no puede superar el subtotal de la linea.");
            }

            var subtotal = subtotalBruto - descuento;
            var ivaValor = Math.Round(subtotal * (producto.PorcentajeIva / 100m), 2);
            var total = subtotal + ivaValor;
            var usuarioIdOperador = producto.ControlaStock ? null : item.UsuarioIdOperador;
            var montoComision = CalculateServiceCommission(producto, usuarioIdOperador, subtotal, item.Cantidad);

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
                PrecioUnitario = precioUnitario,
                Descuento = descuento,
                Subtotal = subtotal,
                IvaValor = ivaValor,
                Total = total,
                UsuarioIdOperador = usuarioIdOperador,
                MontoComisionCalculado = montoComision
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

        if (empresa.ModoDesarrollo)
        {
            var numeroAutorizacion = $"{updatedAt:yyyyMMddHHmmss}{persistedFactura.Secuencial:000000000}";
            const string mensajeModoDesarrollo = "Factura autorizada por simulacion interna de desarrollo.";

            await dbContext.Set<FacturaEntity>()
                .Where(current => current.Id == factura.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(current => current.ClaveAcceso, claveAcceso)
                    .SetProperty(current => current.XmlGenerado, xmlGenerado)
                    .SetProperty(current => current.XmlFirmado, xmlGenerado)
                    .SetProperty(current => current.Estado, FacturaEstado.AUTORIZADO)
                    .SetProperty(current => current.NumeroAutorizacion, numeroAutorizacion)
                    .SetProperty(current => current.MensajeEstado, mensajeModoDesarrollo)
                    .SetProperty(current => current.FechaAutorizacion, updatedAt)
                    .SetProperty(current => current.UpdatedAt, updatedAt),
                    cancellationToken);

            persistedFactura.ClaveAcceso = claveAcceso;
            persistedFactura.XmlGenerado = xmlGenerado;
            persistedFactura.XmlFirmado = xmlGenerado;
            persistedFactura.Estado = FacturaEstado.AUTORIZADO;
            persistedFactura.NumeroAutorizacion = numeroAutorizacion;
            persistedFactura.MensajeEstado = mensajeModoDesarrollo;
            persistedFactura.FechaAutorizacion = updatedAt;
            persistedFactura.UpdatedAt = updatedAt;

            await ApplyInventoryIfNeededAsync(persistedFactura, "Factura autorizada", cancellationToken);

            dbContext.FacturaSriEventos.Add(new FacturaSriEventoEntity
            {
                Id = Guid.NewGuid(),
                FacturaId = persistedFactura.Id,
                Estado = persistedFactura.Estado,
                Mensaje = persistedFactura.MensajeEstado,
                CreatedAt = updatedAt
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new FacturaEmissionResponse
            {
                FacturaId = persistedFactura.Id,
                Secuencial = persistedFactura.Secuencial,
                Estado = persistedFactura.Estado.ToApiValue(),
                NumeroComprobante = $"{persistedFactura.Establecimiento}-{persistedFactura.PuntoEmision}-{persistedFactura.Secuencial:000000000}",
                Mensaje = "Factura registrada, inventario aplicado y comprobante autorizado localmente en modo desarrollo."
            };
        }

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

    public async Task<PagedResultResponse<FacturaMonitorResponse>> GetMonitorAsync(string? term, string? tipoDocumentoId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term?.Trim();
        var normalizedTipoDocumento = tipoDocumentoId?.Trim();
        var incluirFacturas = string.IsNullOrWhiteSpace(normalizedTipoDocumento) || normalizedTipoDocumento == "01";
        var incluirNotasCredito = string.IsNullOrWhiteSpace(normalizedTipoDocumento) || normalizedTipoDocumento == "04";
        var comprobantes = new List<FacturaMonitorResponse>();

        if (incluirFacturas)
        {
            var facturas = await dbContext.Set<FacturaEntity>()
            .AsNoTracking()
            .OrderByDescending(factura => factura.FechaEmision)
            .Select(factura => new FacturaMonitorResponse
            {
                Id = factura.Id,
                Secuencial = factura.Secuencial,
                Establecimiento = factura.Establecimiento,
                PuntoEmision = factura.PuntoEmision,
                TipoDocumentoId = "01",
                TipoDocumentoNombre = "Factura",
                ClienteIdentificacion = factura.ClienteIdentificacion,
                ClienteTipoIdentificacion = factura.ClienteTipoIdentificacion,
                ClienteNombre = factura.ClienteNombre,
                FormaPago = factura.FormaPago,
                Estado = factura.Estado.ToApiValue(),
                Subtotal = factura.Subtotal,
                IvaTotal = factura.IvaTotal,
                Total = factura.Total,
                ClaveAcceso = factura.ClaveAcceso,
                NumeroAutorizacion = factura.NumeroAutorizacion,
                MensajeEstado = factura.MensajeEstado,
                TieneXmlGenerado = factura.XmlGenerado != null && factura.XmlGenerado != string.Empty,
                TieneXmlFirmado = factura.XmlFirmado != null && factura.XmlFirmado != string.Empty,
                FechaEmision = factura.FechaEmision,
                FechaAutorizacion = factura.FechaAutorizacion
            })
            .ToListAsync(cancellationToken);

            comprobantes.AddRange(facturas);
        }

        if (incluirNotasCredito)
        {
            var notasCredito = await dbContext.ComprobanteCabecera
            .AsNoTracking()
            .Where(comprobante => comprobante.TipoDocumentoId == "04")
            .OrderByDescending(comprobante => comprobante.FechaEmision)
            .Select(comprobante => new FacturaMonitorResponse
            {
                Id = comprobante.Id,
                Secuencial = comprobante.Secuencial,
                Establecimiento = comprobante.Establecimiento,
                PuntoEmision = comprobante.PuntoEmision,
                TipoDocumentoId = comprobante.TipoDocumentoId,
                TipoDocumentoNombre = "Nota de credito",
                ClienteIdentificacion = comprobante.ClienteIdentificacion,
                ClienteTipoIdentificacion = comprobante.ClienteTipoIdentificacion,
                ClienteNombre = comprobante.ClienteNombre,
                FormaPago = "Devolucion",
                Estado = comprobante.Estado.ToApiValue(),
                Subtotal = comprobante.Subtotal,
                IvaTotal = comprobante.IvaTotal,
                Total = comprobante.Total,
                ClaveAcceso = comprobante.ClaveAcceso,
                NumeroAutorizacion = comprobante.NumeroAutorizacion,
                MensajeEstado = comprobante.MotivoModificacion,
                TieneXmlGenerado = comprobante.XmlGenerado != null && comprobante.XmlGenerado != string.Empty,
                TieneXmlFirmado = comprobante.XmlFirmado != null && comprobante.XmlFirmado != string.Empty,
                FechaEmision = comprobante.FechaEmision,
                FechaAutorizacion = comprobante.FechaAutorizacion
            })
            .ToListAsync(cancellationToken);

            comprobantes.AddRange(notasCredito);
        }

        if (!string.IsNullOrWhiteSpace(normalizedTerm))
        {
            comprobantes = comprobantes
                .Where(comprobante =>
                comprobante.Establecimiento.Contains(normalizedTerm) ||
                comprobante.PuntoEmision.Contains(normalizedTerm) ||
                comprobante.ClienteNombre.Contains(normalizedTerm) ||
                comprobante.ClienteIdentificacion.Contains(normalizedTerm) ||
                comprobante.ClienteTipoIdentificacion.Contains(normalizedTerm) ||
                comprobante.FormaPago.Contains(normalizedTerm) ||
                comprobante.TipoDocumentoNombre.Contains(normalizedTerm) ||
                comprobante.TipoDocumentoId.Contains(normalizedTerm) ||
                (comprobante.ClaveAcceso != null && comprobante.ClaveAcceso.Contains(normalizedTerm)) ||
                comprobante.Estado.Contains(normalizedTerm) ||
                (comprobante.MensajeEstado != null && comprobante.MensajeEstado.Contains(normalizedTerm)))
                .ToList();
        }

        var totalCount = comprobantes.Count;
        var pageItems = comprobantes
            .OrderByDescending(comprobante => comprobante.FechaEmision)
            .Skip(skip)
            .Take(take)
            .ToArray();

        return new PagedResultResponse<FacturaMonitorResponse>
        {
            Items = pageItems,
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<IReadOnlyCollection<Guid>> GetPendingFacturaIdsAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var staleProcessingLimit = now.AddMinutes(-2);

        return await dbContext.Set<FacturaEntity>()
            .AsNoTracking()
            .Where(factura =>
                (factura.Estado == FacturaEstado.NO_FIRMADO || factura.Estado == FacturaEstado.PENDIENTE) &&
                factura.ClaveAcceso != string.Empty &&
                factura.XmlGenerado != null &&
                factura.NumeroAutorizacion == null &&
                ((factura.Estado == FacturaEstado.NO_FIRMADO && factura.XmlFirmado == null) ||
                 (factura.Estado == FacturaEstado.PENDIENTE && factura.XmlFirmado != null)) &&
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
                (factura.Estado == FacturaEstado.NO_FIRMADO || factura.Estado == FacturaEstado.PENDIENTE) &&
                factura.ClaveAcceso != string.Empty &&
                factura.XmlGenerado != null &&
                factura.NumeroAutorizacion == null &&
                ((factura.Estado == FacturaEstado.NO_FIRMADO && factura.XmlFirmado == null) ||
                 (factura.Estado == FacturaEstado.PENDIENTE && factura.XmlFirmado != null)) &&
                (!factura.NextRetryAt.HasValue || factura.NextRetryAt <= now || factura.ProcessingStartedAt < staleProcessingLimit))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(factura => factura.ProcessingNode, workerId)
                .SetProperty(factura => factura.ProcessingStartedAt, now)
                .SetProperty(factura => factura.UpdatedAt, now)
                .SetProperty(factura => factura.RetryCount, factura => factura.RetryCount + 1)
                .SetProperty(factura => factura.MensajeEstado, factura => factura.Estado == FacturaEstado.PENDIENTE
                    ? "Factura retomada por un worker para consultar autorizacion o reintentar el envio."
                    : "Factura tomada por un worker para firma electronica."),
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
        factura.RetryCount = 0;
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
        string? auditoriaJson,
        TimeSpan? retryDelay,
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
        factura.NextRetryAt = fechaRespuesta.Add(retryDelay ?? ComputeRetryDelay(factura.RetryCount));
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
            Mensaje = ToAuditMessage(mensaje, auditoriaJson),
            CreatedAt = fechaRespuesta
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFacturaAsRejectedAsync(
        Guid facturaId,
        string mensaje,
        string? auditoriaJson,
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
        factura.RetryCount = 0;
        factura.NextRetryAt = null;
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
            Mensaje = ToAuditMessage(mensaje, auditoriaJson),
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
        factura.RetryCount = 0;
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
        string? auditoriaJson,
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
        factura.Estado = FacturaEstado.PENDIENTE;
        factura.ProcessingNode = null;
        factura.ProcessingStartedAt = null;
        factura.NextRetryAt = DateTimeOffset.UtcNow.Add(ComputeRetryDelay(factura.RetryCount));
        factura.EventosSri.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = FacturaEstado.PENDIENTE,
            Mensaje = ToAuditMessage(mensaje, auditoriaJson),
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
            factura.BodegaId,
            $"{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}",
            concepto,
            factura.Detalles.Select(detalle => (detalle.ProductoId, detalle.Cantidad)).ToArray(),
            cancellationToken);

        var inventoryAppliedAt = DateTimeOffset.UtcNow;
        var facturaEntry = dbContext.Entry(factura);
        if (facturaEntry.State != EntityState.Detached)
        {
            factura.InventarioAplicado = true;
            factura.InventarioAplicadoAt = inventoryAppliedAt;
            factura.UpdatedAt = inventoryAppliedAt;
        }
        else
        {
            await dbContext.Set<FacturaEntity>()
                .Where(current => current.Id == factura.Id && !current.InventarioAplicado)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(current => current.InventarioAplicado, true)
                    .SetProperty(current => current.InventarioAplicadoAt, inventoryAppliedAt)
                    .SetProperty(current => current.UpdatedAt, inventoryAppliedAt),
                    cancellationToken);
        }

        var inventoryEvent = new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = factura.Id,
            Estado = factura.Estado,
            Mensaje = $"Inventario transaccional aplicado. Referencia {factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}.",
            CreatedAt = inventoryAppliedAt
        };

        if (facturaEntry.State != EntityState.Detached)
        {
            factura.EventosSri.Add(inventoryEvent);
        }
        else
        {
            dbContext.FacturaSriEventos.Add(inventoryEvent);
            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }
    }

    private async Task<string> BuildDisponibilidadAlternaMessageAsync(
        Guid productoId,
        Guid bodegaActualId,
        decimal cantidadFaltante,
        CancellationToken cancellationToken)
    {
        var alternativas = await dbContext.ProductosBodega
            .AsNoTracking()
            .Include(current => current.Bodega)
            .Where(current =>
                current.ProductoId == productoId &&
                current.BodegaId != bodegaActualId &&
                current.StockActual > 0 &&
                current.Bodega.IsActive)
            .OrderByDescending(current => current.StockActual)
            .ThenBy(current => current.Bodega.Nombre)
            .Take(3)
            .Select(current => new
            {
                current.Bodega.Nombre,
                current.StockActual
            })
            .ToArrayAsync(cancellationToken);

        if (alternativas.Length == 0)
        {
            return " No existe disponibilidad en otras bodegas activas.";
        }

        var detalle = string.Join(
            "; ",
            alternativas.Select(current => $"{current.Nombre}: {current.StockActual:0.####}"));

        return $" Faltante: {cantidadFaltante:0.####}. Disponible en otras bodegas: {detalle}. Registra una transferencia interna antes de facturar.";
    }

    private async Task EnsurePuntoEmisionAllowedForCurrentUserAsync(
        Guid usuarioId,
        Guid empresaId,
        Guid puntoEmisionId,
        CancellationToken cancellationToken)
    {
        var roles = await GetCurrentUserRolesAsync(cancellationToken);
        if (roles.Contains(SecurityRoleNames.Administrador, StringComparer.OrdinalIgnoreCase) ||
            !roles.Contains(SecurityRoleNames.Cajero, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var isAllowed = await dbContext.Set<SecurityUserPuntoEmisionEntity>()
            .AsNoTracking()
            .AnyAsync(current =>
                current.SecurityUserId == usuarioId &&
                current.EmpresaId == empresaId &&
                current.EmpresaPuntoEmisionId == puntoEmisionId,
                cancellationToken);

        if (!isAllowed)
        {
            throw new InvalidOperationException("El cajero autenticado no tiene permiso para operar con el punto de emision seleccionado.");
        }
    }

    private async Task<IReadOnlyCollection<string>> GetCurrentUserRolesAsync(CancellationToken cancellationToken)
    {
        var usuarioId = tenantContextAccessor.UserId;
        if (!usuarioId.HasValue)
        {
            return Array.Empty<string>();
        }

        return await dbContext.SecurityUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(current => current.Id == usuarioId.Value)
            .SelectMany(current => current.UserRoles.Select(userRole => userRole.Role.Name))
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }
    private async Task EnsureOperadoresAllowedForActiveEmpresaAsync(
        IReadOnlyCollection<Guid> operadorIds,
        Guid empresaId,
        CancellationToken cancellationToken)
    {
        if (operadorIds.Count == 0)
        {
            return;
        }

        var operadoresValidos = await dbContext.SecurityUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(usuario =>
                operadorIds.Contains(usuario.Id) &&
                usuario.IsActive &&
                !usuario.BloqueadoManualmente &&
                (usuario.BloqueadoHasta == null || usuario.BloqueadoHasta <= DateTimeOffset.UtcNow) &&
                (usuario.EmpresaId == empresaId || usuario.EmpresasAcceso.Any(acceso => acceso.EmpresaId == empresaId)))
            .Select(usuario => usuario.Id)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        if (operadoresValidos.Length != operadorIds.Count)
        {
            throw new InvalidOperationException("Uno o varios operadores seleccionados no pertenecen a la empresa activa o no estan disponibles.");
        }
    }

    private static string ToAuditMessage(string mensaje, string? auditoriaJson)
    {
        var value = string.IsNullOrWhiteSpace(auditoriaJson) ? mensaje : auditoriaJson;
        return value.Length <= 380 ? value : value[..380];
    }

    private static TimeSpan ComputeRetryDelay(int retryCount)
    {
        var safeRetryCount = Math.Max(1, retryCount);
        var exponent = Math.Min(safeRetryCount - 1, 6);
        var delaySeconds = 15 * Math.Pow(2, exponent);
        var boundedDelaySeconds = Math.Min(delaySeconds, MaxRetryDelayMinutes * 60);
        return TimeSpan.FromSeconds(boundedDelaySeconds);
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
        return SriCatalogCodes.NormalizeTipoIdentificacionCode(tipoIdentificacion)
            ?? throw new InvalidOperationException("El cliente no tiene un tipo de identificacion compatible con la ficha tecnica del SRI.");
    }

    private static string MapFormaPagoSriCodigo(string formaPago)
    {
        return SriCatalogCodes.NormalizeFormaPagoCode(formaPago)
            ?? throw new InvalidOperationException("La forma de pago seleccionada no esta mapeada a un codigo SRI valido.");
    }

    private async Task<long> ReserveNextSecuencialAsync(
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
        return record.UltimoSecuencial;
    }

    private async Task<Guid> ResolvePrincipalBodegaIdAsync(CancellationToken cancellationToken)
    {
        var empresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para resolver la bodega principal.");

        var bodegaId = await dbContext.Bodegas
            .AsNoTracking()
            .Where(current => current.EmpresaId == empresaId && current.Nombre == PrincipalBodegaName && current.IsActive)
            .Select(current => current.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (bodegaId == Guid.Empty)
        {
            throw new InvalidOperationException("No existe una bodega principal configurada para la empresa activa.");
        }

        return bodegaId;
    }


    private static decimal CalculateServiceCommission(ProductoEntity producto, Guid? usuarioIdOperador, decimal subtotal, decimal cantidad)
    {
        if (producto.ControlaStock)
        {
            return 0m;
        }

        if (!usuarioIdOperador.HasValue || usuarioIdOperador.Value == Guid.Empty)
        {
            throw new InvalidOperationException($"Debe asignar un operador para el servicio {producto.Nombre}.");
        }

        if (!producto.AplicaComision)
        {
            return 0m;
        }

        var valorComision = producto.ValorComision ?? 0m;
        var tipoComision = producto.TipoComision ?? string.Empty;
        var monto = tipoComision.Equals("Porcentaje", StringComparison.OrdinalIgnoreCase)
            ? subtotal * (valorComision / 100m)
            : valorComision * cantidad;

        return Math.Round(monto, 2, MidpointRounding.AwayFromZero);
    }
    private async Task<Guid> ResolveOperationalBodegaIdAsync(Guid? requestedBodegaId, CancellationToken cancellationToken)
    {
        if (requestedBodegaId.HasValue && requestedBodegaId.Value != Guid.Empty)
        {
            var empresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para resolver la bodega operativa.");
            var bodega = await dbContext.Bodegas
                .AsNoTracking()
                .FirstOrDefaultAsync(current => current.Id == requestedBodegaId.Value && current.EmpresaId == empresaId, cancellationToken)
                ?? throw new InvalidOperationException("La bodega seleccionada no pertenece a la empresa activa.");

            if (!bodega.IsActive)
            {
                throw new InvalidOperationException("La bodega seleccionada no se encuentra activa.");
            }

            return bodega.Id;
        }

        return await ResolvePrincipalBodegaIdAsync(cancellationToken);
    }
}





