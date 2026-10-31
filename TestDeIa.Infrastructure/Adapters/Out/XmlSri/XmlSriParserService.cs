using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Application.Modules.XmlSri.Ports.In;
using TestDeIa.Domain.Modules.XmlSri.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Compras;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Requests.XmlSri;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Compras;
using TestDeIa.Shared.Responses.XmlSri;

namespace TestDeIa.Infrastructure.Adapters.Out.XmlSri;

public sealed class XmlSriParserService(
    TestDeIaDbContext dbContext,
    ITenantContextAccessor tenantContextAccessor,
    ICompraUseCase compraUseCase,
    ILogger<XmlSriParserService> logger) : IXmlSriParserService
{
    public XmlSriParserResultDto ParsearXmlFacturaSri(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            throw new InvalidOperationException("El XML recibido esta vacio.");
        }

        var document = XDocument.Parse(xmlContent, LoadOptions.PreserveWhitespace);
        var facturaDocument = ExtractFacturaDocument(document);
        var root = facturaDocument.Root ?? throw new InvalidOperationException("El XML no contiene una factura SRI valida.");

        var infoTributaria = Child(root, "infoTributaria");
        var infoFactura = Child(root, "infoFactura");
        var detalles = Child(root, "detalles");

        var result = new XmlSriParserResultDto
        {
            ClaveAcceso = Value(infoTributaria, "claveAcceso"),
            Ambiente = Value(infoTributaria, "ambiente"),
            TipoEmision = Value(infoTributaria, "tipoEmision"),
            RazonSocialEmisor = Value(infoTributaria, "razonSocial"),
            NombreComercialEmisor = Value(infoTributaria, "nombreComercial"),
            RucEmisor = Value(infoTributaria, "ruc"),
            CodDoc = Value(infoTributaria, "codDoc"),
            Establecimiento = Value(infoTributaria, "estab"),
            PuntoEmision = Value(infoTributaria, "ptoEmi"),
            Secuencial = Value(infoTributaria, "secuencial"),
            DireccionMatriz = Value(infoTributaria, "dirMatriz"),
            FechaEmision = ParseDate(Value(infoFactura, "fechaEmision")),
            DireccionEstablecimiento = Value(infoFactura, "dirEstablecimiento"),
            TipoIdentificacionComprador = Value(infoFactura, "tipoIdentificacionComprador"),
            RazonSocialComprador = Value(infoFactura, "razonSocialComprador"),
            RucComprador = Value(infoFactura, "identificacionComprador"),
            TotalSinImpuestos = ParseDecimal(Value(infoFactura, "totalSinImpuestos")),
            TotalDescuento = ParseDecimal(Value(infoFactura, "totalDescuento")),
            Propina = ParseDecimal(Value(infoFactura, "propina")),
            ImporteTotal = ParseDecimal(Value(infoFactura, "importeTotal")),
            Moneda = string.IsNullOrWhiteSpace(Value(infoFactura, "moneda")) ? "DOLAR" : Value(infoFactura, "moneda"),
            XmlContenido = facturaDocument.ToString(SaveOptions.DisableFormatting)
        };

        result.TotalConImpuestos = infoFactura
            .Descendants()
            .Where(current => current.Name.LocalName == "totalImpuesto")
            .Select(current => new XmlSriImpuestoTotalDto
            {
                Codigo = Value(current, "codigo"),
                CodigoPorcentaje = Value(current, "codigoPorcentaje"),
                BaseImponible = ParseDecimal(Value(current, "baseImponible")),
                Valor = ParseDecimal(Value(current, "valor"))
            })
            .ToArray();

        result.Detalles = detalles
            .Elements()
            .Where(current => current.Name.LocalName == "detalle")
            .Select(current => new XmlSriDetalleFacturaDto
            {
                CodigoPrincipal = Value(current, "codigoPrincipal"),
                CodigoAuxiliar = OptionalValue(current, "codigoAuxiliar"),
                Descripcion = Value(current, "descripcion"),
                Cantidad = ParseDecimal(Value(current, "cantidad")),
                PrecioUnitario = ParseDecimal(Value(current, "precioUnitario")),
                Descuento = ParseDecimal(Value(current, "descuento")),
                PrecioTotalSinImpuesto = ParseDecimal(Value(current, "precioTotalSinImpuesto")),
                Impuestos = current
                    .Descendants()
                    .Where(node => node.Name.LocalName == "impuesto")
                    .Select(node => new XmlSriDetalleImpuestoDto
                    {
                        Codigo = Value(node, "codigo"),
                        CodigoPorcentaje = Value(node, "codigoPorcentaje"),
                        Tarifa = ParseDecimal(Value(node, "tarifa")),
                        BaseImponible = ParseDecimal(Value(node, "baseImponible")),
                        Valor = ParseDecimal(Value(node, "valor"))
                    })
                    .ToArray()
            })
            .ToArray();

        result.InformacionAdicional = root
            .Descendants()
            .Where(current => current.Name.LocalName == "campoAdicional")
            .Where(current => current.Attribute("nombre") is not null)
            .GroupBy(current => current.Attribute("nombre")!.Value)
            .ToDictionary(group => group.Key, group => group.First().Value);

        ValidateParsed(result);
        return result;
    }

    public async Task<XmlSriCargaMasivaResponse> ProcesarArchivosXmlMasivosAsync(
        IReadOnlyCollection<string> xmlsContent,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        await EnsureFacturaCompraXmlLogTableAsync(cancellationToken);

        var messages = new List<string>();
        var response = new XmlSriCargaMasivaResponse { Recibidos = xmlsContent.Count };

        foreach (var xml in xmlsContent)
        {
            try
            {
                var parsed = ParsearXmlFacturaSri(xml);
                var existing = await dbContext.FacturaCompraXmlLogs
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.ClaveAcceso == parsed.ClaveAcceso, cancellationToken);

                if (existing is null)
                {
                    dbContext.FacturaCompraXmlLogs.Add(BuildLogEntity(parsed, empresaId, xml));
                    response.Insertados++;
                }
                else if (existing.EstadoProcesamiento == EstadoProcesamientoXmlSri.Pendiente)
                {
                    UpdateLogEntity(existing, parsed, xml);
                    response.Actualizados++;
                }
                else
                {
                    response.Duplicados++;
                }
            }
            catch (Exception exception)
            {
                response.Errores++;
                messages.Add(exception.Message);
                logger.LogWarning(exception, "No se pudo procesar uno de los XML SRI de compras.");
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        response.Mensajes = messages;
        return response;
    }

    public async Task<PagedResultResponse<FacturaCompraXmlLogResponse>> GetPendientesAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        await EnsureFacturaCompraXmlLogTableAsync(cancellationToken);

        var query = dbContext.FacturaCompraXmlLogs
            .AsNoTracking()
            .Where(current => current.EstadoProcesamiento == EstadoProcesamientoXmlSri.Pendiente);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(current => current.FechaEmision)
            .Skip(skip)
            .Take(take)
            .Select(current => Map(current))
            .ToArrayAsync(cancellationToken);

        return new PagedResultResponse<FacturaCompraXmlLogResponse>
        {
            Items = items,
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public async Task<CompraResponse> ConvertirXmlACompraAsync(
        ConvertirXmlACompraRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureFacturaCompraXmlLogTableAsync(cancellationToken);

        var log = await dbContext.FacturaCompraXmlLogs
            .FirstOrDefaultAsync(current => current.Id == request.FacturaCompraXmlLogId, cancellationToken)
            ?? throw new InvalidOperationException("El XML de compra no existe o no pertenece a la empresa activa.");

        if (log.EstadoProcesamiento == EstadoProcesamientoXmlSri.ProcesadoACompra)
        {
            throw new InvalidOperationException("El XML ya fue convertido a compra.");
        }

        var parsed = ParsearXmlFacturaSri(log.XmlContenido);
        var empresaId = tenantContextAccessor.EmpresaId ?? log.EmpresaId;
        var proveedorId = request.ProveedorId ?? await ResolveProveedorIdAsync(empresaId, parsed, request.CrearProveedorSiNoExiste, cancellationToken);
        var mapByProviderCode = request.MapeosProductos
            .Where(current => current.ProductoId != Guid.Empty)
            .GroupBy(current => current.CodigoProductoProveedor.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().ProductoId, StringComparer.OrdinalIgnoreCase);

        var detalles = new List<RegistrarCompraDetalleRequest>();
        foreach (var item in parsed.Detalles)
        {
            var productoId = await ResolveProductoIdAsync(item, mapByProviderCode, cancellationToken);
            if (!productoId.HasValue)
            {
                log.EstadoProcesamiento = EstadoProcesamientoXmlSri.ErrorMapeo;
                log.UpdatedAt = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException($"No se encontro producto interno para el item XML: {item.CodigoPrincipal} - {item.Descripcion}.");
            }

            detalles.Add(new RegistrarCompraDetalleRequest
            {
                ProductoId = productoId.Value,
                NaturalezaCompra = NaturalezaCompra.MercaderiaInventario,
                Cantidad = item.Cantidad,
                CostoUnitario = item.Cantidad == 0 ? item.PrecioUnitario : Math.Round(item.PrecioTotalSinImpuesto / item.Cantidad, 6, MidpointRounding.AwayFromZero),
                Descuento = item.Descuento
            });
        }

        var compraRequest = new RegistrarCompraRequest
        {
            ProveedorId = proveedorId,
            BodegaId = request.BodegaId,
            NaturalezaCompra = NaturalezaCompra.MercaderiaInventario,
            FormaPagoCompra = request.FormaPagoCompra,
            RequiereBancarizacion = parsed.ImporteTotal >= 1000m,
            TipoDocumentoCodigo = "01",
            TipoComprobanteSRI = parsed.CodDoc,
            SustentoTributarioSRI = "01",
            NumeroComprobante = parsed.EstabPuntoEmiSecuencial,
            ClaveAccesoProveedor = parsed.ClaveAcceso,
            NumeroAutorizacion = parsed.ClaveAcceso,
            Establecimiento = parsed.Establecimiento,
            PuntoEmision = parsed.PuntoEmision,
            FormaPago = request.FormaPagoCompra == FormaPagoCompra.ContadoEfectivo ? "01" : "20",
            Observacion = $"Compra importada desde XML SRI {parsed.ClaveAcceso}",
            DiasCredito = request.DiasCredito,
            FechaEmision = new DateTimeOffset(parsed.FechaEmision.Date, TimeSpan.Zero),
            Detalles = detalles
        };

        var compra = await compraUseCase.RegistrarAsync(compraRequest, cancellationToken);
        log.EstadoProcesamiento = EstadoProcesamientoXmlSri.ProcesadoACompra;
        log.CompraId = compra.Id;
        log.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return compra;
    }

    public async Task<XmlSriParserResultDto> GetParsedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var xml = await GetXmlOriginalAsync(id, cancellationToken);
        return ParsearXmlFacturaSri(xml);
    }

    public async Task<string> GetXmlOriginalAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureFacturaCompraXmlLogTableAsync(cancellationToken);

        var xml = await dbContext.FacturaCompraXmlLogs
            .AsNoTracking()
            .Where(current => current.Id == id)
            .Select(current => current.XmlContenido)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(xml)
            ? throw new InvalidOperationException("El XML solicitado no existe o no pertenece a la empresa activa.")
            : xml;
    }

    private async Task<Guid> ResolveProveedorIdAsync(
        Guid empresaId,
        XmlSriParserResultDto parsed,
        bool createIfMissing,
        CancellationToken cancellationToken)
    {
        var proveedor = await dbContext.Proveedores
            .Include(current => current.Persona)
            .FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.Persona.Identificacion == parsed.RucEmisor, cancellationToken);

        if (proveedor is not null)
        {
            return proveedor.PersonaId;
        }

        if (!createIfMissing)
        {
            throw new InvalidOperationException($"No existe proveedor con RUC {parsed.RucEmisor}.");
        }

        var persona = new PersonaEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            TipoIdentificacion = "04",
            Identificacion = parsed.RucEmisor,
            RazonSocialONombresCompletos = parsed.RazonSocialEmisor,
            DireccionPrincipal = parsed.DireccionMatriz,
            CorreoElectronicoPrincipal = parsed.InformacionAdicional.TryGetValue("Email", out var email) ? email : null,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Personas.Add(persona);
        dbContext.Proveedores.Add(new ProveedorEntity
        {
            PersonaId = persona.Id,
            EmpresaId = empresaId,
            CodigoRetencionIvaDefault = "3440",
            CodigoRetencionRentaDefault = "312",
            PermiteCredito = true,
            DiasCredito = 30,
            EstadoProveedor = "Activo",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return persona.Id;
    }

    private async Task EnsureFacturaCompraXmlLogTableAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[FacturaCompraXmlLogs](
                    [Id] uniqueidentifier NOT NULL,
                    [EmpresaId] uniqueidentifier NOT NULL,
                    [ClaveAcceso] nvarchar(49) NOT NULL,
                    [RucEmisor] nvarchar(13) NOT NULL,
                    [RazonSocialEmisor] nvarchar(300) NOT NULL,
                    [RucComprador] nvarchar(13) NOT NULL,
                    [FechaEmision] datetimeoffset NOT NULL,
                    [CodDoc] nvarchar(2) NOT NULL,
                    [EstabPuntoEmiSecuencial] nvarchar(17) NOT NULL,
                    [TotalSinImpuestos] decimal(18,4) NOT NULL,
                    [TotalDescuento] decimal(18,4) NOT NULL,
                    [ImporteTotal] decimal(18,4) NOT NULL,
                    [XmlContenido] nvarchar(max) NOT NULL,
                    [EstadoProcesamiento] tinyint NOT NULL,
                    [CompraId] uniqueidentifier NULL,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [UpdatedAt] datetimeoffset NULL,
                    CONSTRAINT [PK_FacturaCompraXmlLogs] PRIMARY KEY ([Id])
                );
            END
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[Compras]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_FacturaCompraXmlLogs_Compras_CompraId')
            BEGIN
                ALTER TABLE [dbo].[FacturaCompraXmlLogs]
                ADD CONSTRAINT [FK_FacturaCompraXmlLogs_Compras_CompraId]
                FOREIGN KEY ([CompraId]) REFERENCES [dbo].[Compras]([Id]) ON DELETE SET NULL;
            END
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_CompraId' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
            BEGIN
                CREATE INDEX [IX_FacturaCompraXmlLogs_CompraId] ON [dbo].[FacturaCompraXmlLogs]([CompraId]);
            END
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_EmpresaId_ClaveAcceso' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
            BEGIN
                CREATE UNIQUE INDEX [IX_FacturaCompraXmlLogs_EmpresaId_ClaveAcceso]
                ON [dbo].[FacturaCompraXmlLogs]([EmpresaId], [ClaveAcceso]);
            END
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_EmpresaId_EstadoProcesamiento_FechaEmision' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
            BEGIN
                CREATE INDEX [IX_FacturaCompraXmlLogs_EmpresaId_EstadoProcesamiento_FechaEmision]
                ON [dbo].[FacturaCompraXmlLogs]([EmpresaId], [EstadoProcesamiento], [FechaEmision]);
            END
            """,
            cancellationToken);
    }

    private async Task<Guid?> ResolveProductoIdAsync(
        XmlSriDetalleFacturaDto item,
        IReadOnlyDictionary<string, Guid> mapByProviderCode,
        CancellationToken cancellationToken)
    {
        if (mapByProviderCode.TryGetValue(item.CodigoPrincipal.Trim(), out var mappedId))
        {
            return mappedId;
        }

        var codigo = item.CodigoPrincipal.Trim();
        var descripcion = item.Descripcion.Trim();
        var producto = await dbContext.Productos
            .AsNoTracking()
            .Where(current =>
                current.IsActive &&
                (current.Codigo == codigo ||
                 current.Nombre == descripcion ||
                 current.Nombre.Contains(descripcion) ||
                 descripcion.Contains(current.Nombre)))
            .OrderBy(current => current.Nombre == descripcion ? 0 : 1)
            .ThenBy(current => current.Codigo == codigo ? 0 : 1)
            .FirstOrDefaultAsync(cancellationToken);

        return producto?.Id;
    }

    private static FacturaCompraXmlLogEntity BuildLogEntity(XmlSriParserResultDto parsed, Guid empresaId, string originalXml)
    {
        return new FacturaCompraXmlLogEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ClaveAcceso = parsed.ClaveAcceso,
            RucEmisor = parsed.RucEmisor,
            RazonSocialEmisor = parsed.RazonSocialEmisor,
            RucComprador = parsed.RucComprador,
            FechaEmision = new DateTimeOffset(parsed.FechaEmision.Date, TimeSpan.Zero),
            CodDoc = parsed.CodDoc,
            EstabPuntoEmiSecuencial = parsed.EstabPuntoEmiSecuencial,
            TotalSinImpuestos = parsed.TotalSinImpuestos,
            TotalDescuento = parsed.TotalDescuento,
            ImporteTotal = parsed.ImporteTotal,
            XmlContenido = originalXml,
            EstadoProcesamiento = EstadoProcesamientoXmlSri.Pendiente,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static void UpdateLogEntity(FacturaCompraXmlLogEntity entity, XmlSriParserResultDto parsed, string originalXml)
    {
        entity.RucEmisor = parsed.RucEmisor;
        entity.RazonSocialEmisor = parsed.RazonSocialEmisor;
        entity.RucComprador = parsed.RucComprador;
        entity.FechaEmision = new DateTimeOffset(parsed.FechaEmision.Date, TimeSpan.Zero);
        entity.CodDoc = parsed.CodDoc;
        entity.EstabPuntoEmiSecuencial = parsed.EstabPuntoEmiSecuencial;
        entity.TotalSinImpuestos = parsed.TotalSinImpuestos;
        entity.TotalDescuento = parsed.TotalDescuento;
        entity.ImporteTotal = parsed.ImporteTotal;
        entity.XmlContenido = originalXml;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static FacturaCompraXmlLogResponse Map(FacturaCompraXmlLogEntity entity)
    {
        return new FacturaCompraXmlLogResponse
        {
            Id = entity.Id,
            EmpresaId = entity.EmpresaId,
            ClaveAcceso = entity.ClaveAcceso,
            RucEmisor = entity.RucEmisor,
            RazonSocialEmisor = entity.RazonSocialEmisor,
            RucComprador = entity.RucComprador,
            FechaEmision = entity.FechaEmision.Date,
            CodDoc = entity.CodDoc,
            EstabPuntoEmiSecuencial = entity.EstabPuntoEmiSecuencial,
            TotalSinImpuestos = entity.TotalSinImpuestos,
            TotalDescuento = entity.TotalDescuento,
            ImporteTotal = entity.ImporteTotal,
            EstadoProcesamiento = (int)entity.EstadoProcesamiento,
            EstadoProcesamientoNombre = entity.EstadoProcesamiento.ToString(),
            CompraId = entity.CompraId
        };
    }

    private static XDocument ExtractFacturaDocument(XDocument document)
    {
        if (document.Root?.Name.LocalName == "factura")
        {
            return document;
        }

        var comprobanteText = document
            .Descendants()
            .FirstOrDefault(current => current.Name.LocalName == "comprobante")
            ?.Value;

        if (!string.IsNullOrWhiteSpace(comprobanteText))
        {
            return XDocument.Parse(comprobanteText, LoadOptions.PreserveWhitespace);
        }

        var facturaNode = document.Descendants().FirstOrDefault(current => current.Name.LocalName == "factura");
        if (facturaNode is not null)
        {
            return new XDocument(facturaNode);
        }

        throw new InvalidOperationException("No se encontro el nodo factura dentro del XML SRI.");
    }

    private static XElement Child(XElement parent, string localName)
    {
        return parent.Elements().FirstOrDefault(current => current.Name.LocalName == localName)
            ?? throw new InvalidOperationException($"No se encontro el nodo obligatorio {localName}.");
    }

    private static string Value(XElement parent, string localName)
    {
        return parent.Elements().FirstOrDefault(current => current.Name.LocalName == localName)?.Value?.Trim() ?? string.Empty;
    }

    private static string? OptionalValue(XElement parent, string localName)
    {
        var value = Value(parent, localName);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static decimal ParseDecimal(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0m;
    }

    private static DateTime ParseDate(string value)
    {
        var formats = new[] { "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };
        return DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : DateTime.Parse(value, CultureInfo.InvariantCulture);
    }

    private static void ValidateParsed(XmlSriParserResultDto result)
    {
        if (result.CodDoc != "01")
        {
            throw new InvalidOperationException("Solo se admite XML SRI de factura de compra codigo 01.");
        }

        if (result.ClaveAcceso.Length != 49)
        {
            throw new InvalidOperationException("La clave de acceso del XML debe contener 49 digitos.");
        }

        if (string.IsNullOrWhiteSpace(result.RucEmisor) || string.IsNullOrWhiteSpace(result.RazonSocialEmisor))
        {
            throw new InvalidOperationException("El XML no contiene datos completos del emisor.");
        }

        if (result.Detalles.Count == 0)
        {
            throw new InvalidOperationException("El XML no contiene detalles de factura.");
        }
    }
}
