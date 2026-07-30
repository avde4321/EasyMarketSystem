using Microsoft.EntityFrameworkCore;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Reports.Facturacion;

namespace TestDeIa.Api.Reporting;

public sealed class FacturaDocumentQueryService
{
    private readonly TestDeIaDbContext dbContext;

    public FacturaDocumentQueryService(TestDeIaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<FacturaRideReportModel?> GetFacturaRideAsync(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await dbContext.Facturas
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken);

        if (factura is null)
        {
            return null;
        }

        var empresa = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .FirstOrDefaultAsync(current =>
                current.Id == (factura.EmpresaEmisoraId ?? factura.EmpresaId),
                cancellationToken);

        var bannerPath = Path.Combine(AppContext.BaseDirectory, "Reporting", "Templates", "SriFacturaBanner.png");
        var subtotalesPorTarifa = factura.Detalles
            .GroupBy(current => current.PorcentajeIva)
            .ToDictionary(
                group => group.Key,
                group => Math.Round(group.Sum(current => current.Subtotal), 2));

        var ivaPorTarifa = factura.Detalles
            .GroupBy(current => current.PorcentajeIva)
            .ToDictionary(
                group => group.Key,
                group => Math.Round(group.Sum(current => current.IvaValor), 2));

        var tarifaPrincipalIva = subtotalesPorTarifa
            .Where(current => current.Key > 0m && current.Value > 0m)
            .OrderByDescending(current => current.Value)
            .ThenByDescending(current => current.Key)
            .Select(current => current.Key)
            .FirstOrDefault();

        var subtotalTarifaPrincipal = tarifaPrincipalIva > 0m && subtotalesPorTarifa.TryGetValue(tarifaPrincipalIva, out var subtotalPrincipal)
            ? subtotalPrincipal
            : 0m;

        var totalSubsidio = 0m;
        var detalleRows = factura.Detalles
            .OrderBy(current => current.NombreProducto)
            .Select(current => new FacturaRideDetalleRow
            {
                CodigoPrincipal = current.CodigoProducto,
                CodigoAuxiliar = string.Empty,
                Cantidad = current.Cantidad.ToString("0.####"),
                Descripcion = current.NombreProducto,
                DetalleAdicional1 = string.Empty,
                DetalleAdicional2 = string.Empty,
                DetalleAdicional3 = string.Empty,
                PrecioUnitario = current.PrecioUnitario.ToString("0.00"),
                Subsidio = "0.00",
                PrecioSinSubsidio = "0.00",
                Descuento = "0.00",
                PrecioTotal = current.Total.ToString("0.00")
            })
            .ToArray();

        var totalesRows = new List<FacturaRideTotalRow>
        {
            new() { Label = $"SUBTOTAL {FormatPercentageLabel(tarifaPrincipalIva)}", Valor = subtotalTarifaPrincipal.ToString("0.00") },
            new() { Label = "SUBTOTAL IVA 0%", Valor = factura.SubtotalIva0.ToString("0.00") },
            new() { Label = "SUBTOTAL NO OBJETO IVA", Valor = "0.00" },
            new() { Label = "SUBTOTAL EXENTO IVA", Valor = "0.00" },
            new() { Label = "SUBTOTAL SIN IMPUESTOS", Valor = factura.Subtotal.ToString("0.00") },
            new() { Label = "DESCUENTO", Valor = factura.TotalDescuento.ToString("0.00") },
            new() { Label = "ICE", Valor = "0.00" },
            new() { Label = $"IVA {FormatPercentageLabel(tarifaPrincipalIva)}", Valor = (tarifaPrincipalIva > 0m && ivaPorTarifa.TryGetValue(tarifaPrincipalIva, out var ivaPrincipal) ? ivaPrincipal : 0m).ToString("0.00") },
            new() { Label = "IRBPNR", Valor = "0.00" },
            new() { Label = "PROPINA", Valor = "0.00" },
            new() { Label = "VALOR TOTAL", Valor = factura.Total.ToString("0.00") }
        };

        var razonSocialEmisor = string.IsNullOrWhiteSpace(empresa?.RazonSocial) ? factura.RazonSocialEmisor : empresa.RazonSocial;
        var nombreComercialEmisor = string.IsNullOrWhiteSpace(empresa?.NombreComercial)
            ? (string.IsNullOrWhiteSpace(factura.NombreComercialEmisor) ? razonSocialEmisor : factura.NombreComercialEmisor)
            : empresa.NombreComercial;
        var direccionMatriz = string.IsNullOrWhiteSpace(empresa?.DireccionMatriz) ? factura.DireccionMatrizEmisor : empresa.DireccionMatriz;
        var direccionSucursal = string.IsNullOrWhiteSpace(empresa?.DireccionEstablecimiento)
            ? (string.IsNullOrWhiteSpace(factura.DireccionEstablecimientoEmisor) ? direccionMatriz : factura.DireccionEstablecimientoEmisor)
            : empresa.DireccionEstablecimiento;

        return new FacturaRideReportModel
        {
            FacturaId = factura.Id,
            BannerImagePath = new Uri(bannerPath).AbsoluteUri,
            BannerImageContent = empresa?.LogoRideContenido,
            BannerImageMimeType = empresa?.LogoRideMimeType,
            NumeroComprobante = $"{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}",
            Estado = factura.Estado.ToApiValue(),
            ClaveAcceso = factura.ClaveAcceso,
            NumeroAutorizacion = string.IsNullOrWhiteSpace(factura.NumeroAutorizacion) ? "-" : factura.NumeroAutorizacion,
            FechaEmision = factura.FechaEmision.LocalDateTime.ToString("dd/MM/yyyy"),
            FechaAutorizacion = factura.Estado == FacturaEstado.AUTORIZADO
                ? factura.FechaAutorizacion?.LocalDateTime.ToString("dd/MM/yyyy HH:mm:ss") ?? "-"
                : "-",
            EmisorRazonSocial = razonSocialEmisor,
            EmisorNombreComercial = nombreComercialEmisor,
            EmisorRuc = string.IsNullOrWhiteSpace(empresa?.Ruc) ? factura.RucEmisor : empresa.Ruc,
            EmisorDireccionMatriz = direccionMatriz,
            EmisorDireccionSucursal = string.IsNullOrWhiteSpace(direccionSucursal) ? direccionMatriz : direccionSucursal,
            AmbienteSri = string.IsNullOrWhiteSpace(empresa?.AmbienteSri) ? factura.AmbienteSri : empresa.AmbienteSri,
            EmisionTipo = string.IsNullOrWhiteSpace(empresa?.TipoEmision) ? factura.TipoEmision.ToUpperInvariant() : empresa.TipoEmision.ToUpperInvariant(),
            ObligadoContabilidad = (empresa?.ObligadoContabilidad ?? factura.ObligadoContabilidad) ? "SI" : "NO",
            ContribuyenteEspecial = string.IsNullOrWhiteSpace(empresa?.ContribuyenteEspecial) ? factura.ContribuyenteEspecial ?? string.Empty : empresa.ContribuyenteEspecial,
            ClienteNombre = factura.ClienteNombre,
            ClienteIdentificacion = factura.ClienteIdentificacion,
            ClienteDireccion = string.IsNullOrWhiteSpace(factura.ClienteDireccion) ? "-" : factura.ClienteDireccion,
            ClienteEmail = string.IsNullOrWhiteSpace(factura.ClienteEmail) ? "-" : factura.ClienteEmail,
            ClienteTelefono = string.IsNullOrWhiteSpace(factura.ClienteTelefono) ? "-" : factura.ClienteTelefono,
            FormaPago = factura.FormaPago,
            GuiaRemision = string.Empty,
            Observacion = string.IsNullOrWhiteSpace(factura.Observacion) ? "-" : factura.Observacion,
            WatermarkText = ResolveWatermark(factura.Estado),
            XmlGenerado = factura.XmlGenerado ?? string.Empty,
            XmlFirmado = factura.XmlFirmado,
            Subtotal = factura.Subtotal,
            IvaTotal = factura.IvaTotal,
            Total = factura.Total,
            SubtotalIva12 = subtotalTarifaPrincipal,
            SubtotalIva0 = factura.SubtotalIva0,
            SubtotalNoObjeto = 0m,
            SubtotalExento = 0m,
            SubtotalSinImpuestos = factura.Subtotal,
            Ice = 0m,
            Irbpnr = 0m,
            Propina = 0m,
            Descuento = factura.TotalDescuento,
            TotalSubsidio = totalSubsidio,
            TotalSinSubsidio = factura.Total + totalSubsidio,
            Detalles = detalleRows,
            Totales = totalesRows
        };
    }

    public async Task<FacturaRideReportModel?> GetComprobanteRideAsync(Guid comprobanteId, CancellationToken cancellationToken)
    {
        var factura = await GetFacturaRideAsync(comprobanteId, cancellationToken);
        if (factura is not null)
        {
            return factura;
        }

        var comprobante = await dbContext.ComprobanteCabecera
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == comprobanteId, cancellationToken);

        if (comprobante is null)
        {
            return null;
        }

        var empresa = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == comprobante.EmpresaId, cancellationToken);

        var bannerPath = Path.Combine(AppContext.BaseDirectory, "Reporting", "Templates", "SriFacturaBanner.png");
        var tarifaPrincipalIva = comprobante.Detalles
            .Where(current => current.PorcentajeIva > 0m && current.Subtotal > 0m)
            .OrderByDescending(current => current.Subtotal)
            .ThenByDescending(current => current.PorcentajeIva)
            .Select(current => current.PorcentajeIva)
            .FirstOrDefault();

        var subtotalTarifaPrincipal = comprobante.Detalles
            .Where(current => current.PorcentajeIva == tarifaPrincipalIva)
            .Sum(current => current.Subtotal);

        var ivaTarifaPrincipal = comprobante.Detalles
            .Where(current => current.PorcentajeIva == tarifaPrincipalIva)
            .Sum(current => current.IvaValor);

        var detalleRows = comprobante.Detalles
            .OrderBy(current => current.NombreProducto)
            .Select(current => new FacturaRideDetalleRow
            {
                CodigoPrincipal = current.CodigoProducto,
                CodigoAuxiliar = string.Empty,
                Cantidad = current.Cantidad.ToString("0.####"),
                Descripcion = current.NombreProducto,
                DetalleAdicional1 = comprobante.TipoDocumentoId == "04" ? "Nota de credito" : string.Empty,
                DetalleAdicional2 = string.IsNullOrWhiteSpace(comprobante.NumDocModificado) ? string.Empty : $"Doc. modificado: {comprobante.NumDocModificado}",
                DetalleAdicional3 = string.Empty,
                PrecioUnitario = current.PrecioUnitario.ToString("0.00"),
                Subsidio = "0.00",
                PrecioSinSubsidio = "0.00",
                Descuento = current.Descuento.ToString("0.00"),
                PrecioTotal = current.Total.ToString("0.00")
            })
            .ToArray();

        var subtotalIva0 = comprobante.Detalles
            .Where(current => current.PorcentajeIva == 0m)
            .Sum(current => current.Subtotal);

        var totalesRows = new List<FacturaRideTotalRow>
        {
            new() { Label = $"SUBTOTAL {FormatPercentageLabel(tarifaPrincipalIva)}", Valor = subtotalTarifaPrincipal.ToString("0.00") },
            new() { Label = "SUBTOTAL IVA 0%", Valor = subtotalIva0.ToString("0.00") },
            new() { Label = "SUBTOTAL SIN IMPUESTOS", Valor = comprobante.Subtotal.ToString("0.00") },
            new() { Label = "DESCUENTO", Valor = comprobante.TotalDescuento.ToString("0.00") },
            new() { Label = $"IVA {FormatPercentageLabel(tarifaPrincipalIva)}", Valor = ivaTarifaPrincipal.ToString("0.00") },
            new() { Label = "VALOR TOTAL", Valor = comprobante.Total.ToString("0.00") }
        };

        var numeroComprobante = $"{comprobante.Establecimiento}-{comprobante.PuntoEmision}-{comprobante.Secuencial:000000000}";
        var observacion = comprobante.TipoDocumentoId == "04"
            ? $"NOTA DE CREDITO. Modifica {comprobante.NumDocModificado}. Motivo: {comprobante.MotivoModificacion}"
            : comprobante.MotivoModificacion ?? "-";

        var emisorRazonSocial = string.IsNullOrWhiteSpace(empresa?.RazonSocial) ? comprobante.RazonSocialEmisor : empresa.RazonSocial;
        var emisorNombreComercial = string.IsNullOrWhiteSpace(empresa?.NombreComercial)
            ? (string.IsNullOrWhiteSpace(comprobante.NombreComercialEmisor) ? emisorRazonSocial : comprobante.NombreComercialEmisor)
            : empresa.NombreComercial;
        var emisorDireccionMatriz = string.IsNullOrWhiteSpace(empresa?.DireccionMatriz) ? comprobante.DireccionMatrizEmisor : empresa.DireccionMatriz;
        var emisorDireccionSucursal = string.IsNullOrWhiteSpace(empresa?.DireccionEstablecimiento)
            ? (string.IsNullOrWhiteSpace(comprobante.DireccionEstablecimientoEmisor) ? emisorDireccionMatriz : comprobante.DireccionEstablecimientoEmisor)
            : empresa.DireccionEstablecimiento;

        return new FacturaRideReportModel
        {
            FacturaId = comprobante.Id,
            BannerImagePath = new Uri(bannerPath).AbsoluteUri,
            BannerImageContent = empresa?.LogoRideContenido,
            BannerImageMimeType = empresa?.LogoRideMimeType,
            NumeroComprobante = numeroComprobante,
            Estado = comprobante.Estado.ToApiValue(),
            ClaveAcceso = comprobante.ClaveAcceso,
            NumeroAutorizacion = string.IsNullOrWhiteSpace(comprobante.NumeroAutorizacion) ? "-" : comprobante.NumeroAutorizacion,
            FechaEmision = comprobante.FechaEmision.LocalDateTime.ToString("dd/MM/yyyy"),
            FechaAutorizacion = comprobante.Estado == FacturaEstado.AUTORIZADO
                ? comprobante.FechaAutorizacion?.LocalDateTime.ToString("dd/MM/yyyy HH:mm:ss") ?? "-"
                : "-",
            EmisorRazonSocial = emisorRazonSocial,
            EmisorNombreComercial = emisorNombreComercial,
            EmisorRuc = string.IsNullOrWhiteSpace(empresa?.Ruc) ? comprobante.RucEmisor : empresa.Ruc,
            EmisorDireccionMatriz = emisorDireccionMatriz,
            EmisorDireccionSucursal = emisorDireccionSucursal,
            AmbienteSri = string.IsNullOrWhiteSpace(empresa?.AmbienteSri) ? comprobante.AmbienteSri : empresa.AmbienteSri,
            EmisionTipo = string.IsNullOrWhiteSpace(empresa?.TipoEmision) ? comprobante.TipoEmision.ToUpperInvariant() : empresa.TipoEmision.ToUpperInvariant(),
            ObligadoContabilidad = (empresa?.ObligadoContabilidad ?? comprobante.ObligadoContabilidad) ? "SI" : "NO",
            ContribuyenteEspecial = string.IsNullOrWhiteSpace(empresa?.ContribuyenteEspecial) ? string.Empty : empresa.ContribuyenteEspecial,
            ClienteNombre = comprobante.ClienteNombre,
            ClienteIdentificacion = comprobante.ClienteIdentificacion,
            ClienteDireccion = string.IsNullOrWhiteSpace(comprobante.ClienteDireccion) ? "-" : comprobante.ClienteDireccion,
            ClienteEmail = "-",
            ClienteTelefono = "-",
            FormaPago = comprobante.TipoDocumentoId == "04" ? "NOTA DE CREDITO" : string.Empty,
            GuiaRemision = string.Empty,
            Observacion = observacion,
            WatermarkText = ResolveWatermark(comprobante.Estado),
            XmlGenerado = comprobante.XmlGenerado ?? string.Empty,
            XmlFirmado = comprobante.XmlFirmado,
            Subtotal = comprobante.Subtotal,
            IvaTotal = comprobante.IvaTotal,
            Total = comprobante.Total,
            SubtotalIva12 = subtotalTarifaPrincipal,
            SubtotalIva0 = subtotalIva0,
            SubtotalSinImpuestos = comprobante.Subtotal,
            Descuento = comprobante.TotalDescuento,
            TotalSinSubsidio = comprobante.Total,
            Detalles = detalleRows,
            Totales = totalesRows
        };
    }

    public async Task<NotaCreditoRideReportDto?> GetNotaCreditoRideAsync(Guid comprobanteId, CancellationToken cancellationToken)
    {
        var comprobante = await dbContext.ComprobanteCabecera
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == comprobanteId && current.TipoDocumentoId == "04", cancellationToken);

        if (comprobante is null)
        {
            return null;
        }

        var totales = comprobante.Detalles
            .GroupBy(current => new { current.CodigoIva, current.PorcentajeIva })
            .Select(group => new NotaCreditoRideTotalImpuestoDto
            {
                CodigoIva = group.Key.CodigoIva,
                PorcentajeIva = group.Key.PorcentajeIva,
                BaseImponible = group.Sum(current => current.Subtotal),
                Valor = group.Sum(current => current.IvaValor)
            })
            .ToArray();

        return new NotaCreditoRideReportDto
        {
            ComprobanteId = comprobante.Id,
            NumeroComprobante = $"{comprobante.Establecimiento}-{comprobante.PuntoEmision}-{comprobante.Secuencial:000000000}",
            ClaveAcceso = comprobante.ClaveAcceso,
            NumeroAutorizacion = comprobante.NumeroAutorizacion ?? string.Empty,
            FechaEmision = comprobante.FechaEmision,
            RucEmisor = comprobante.RucEmisor,
            RazonSocialEmisor = comprobante.RazonSocialEmisor,
            NombreComercialEmisor = comprobante.NombreComercialEmisor ?? string.Empty,
            DireccionMatrizEmisor = comprobante.DireccionMatrizEmisor,
            DireccionEstablecimientoEmisor = comprobante.DireccionEstablecimientoEmisor ?? comprobante.DireccionMatrizEmisor,
            ClienteIdentificacion = comprobante.ClienteIdentificacion,
            ClienteNombre = comprobante.ClienteNombre,
            ClienteDireccion = comprobante.ClienteDireccion ?? "-",
            CodDocModificado = comprobante.CodDocModificado ?? string.Empty,
            NumDocModificado = comprobante.NumDocModificado ?? string.Empty,
            FechaEmisionDocSustento = comprobante.FechaEmisionDocSustento ?? comprobante.FechaEmision,
            MotivoModificacion = comprobante.MotivoModificacion ?? string.Empty,
            Subtotal = comprobante.Subtotal,
            TotalDescuento = comprobante.TotalDescuento,
            IvaTotal = comprobante.IvaTotal,
            Total = comprobante.Total,
            Detalles = comprobante.Detalles
                .OrderBy(current => current.NombreProducto)
                .Select(current => new NotaCreditoRideDetalleDto
                {
                    CodigoProducto = current.CodigoProducto,
                    NombreProducto = current.NombreProducto,
                    Cantidad = current.Cantidad,
                    PrecioUnitario = current.PrecioUnitario,
                    Descuento = current.Descuento,
                    Subtotal = current.Subtotal,
                    IvaValor = current.IvaValor,
                    Total = current.Total
                })
                .ToArray(),
            TotalesImpuesto = totales
        };
    }

    public async Task<FacturaEmailNotificationDocument?> GetFacturaEmailNotificationDocumentAsync(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await dbContext.Facturas
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken);

        if (factura is null || factura.Estado != FacturaEstado.AUTORIZADO)
        {
            return null;
        }

        var ride = await GetFacturaRideAsync(facturaId, cancellationToken);
        if (ride is null || string.IsNullOrWhiteSpace(factura.ClienteEmail) || string.IsNullOrWhiteSpace(factura.XmlFirmado))
        {
            return null;
        }

        return new FacturaEmailNotificationDocument
        {
            FacturaId = factura.Id,
            EmpresaId = factura.EmpresaId,
            DestinatarioEmail = factura.ClienteEmail,
            DestinatarioNombre = factura.ClienteNombre,
            NumeroComprobante = $"{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}",
            FechaEmision = factura.FechaEmision,
            Total = factura.Total,
            Ride = ride,
            XmlAutorizado = factura.XmlFirmado
        };
    }

    private static string ResolveWatermark(FacturaEstado estado)
    {
        return estado switch
        {
            FacturaEstado.NO_FIRMADO => "DOCUMENTO NO FIRMADO",
            FacturaEstado.PENDIENTE => "PENDIENTE DE AUTORIZACION",
            _ => string.Empty
        };
    }

    private static string FormatPercentageLabel(decimal percentage)
    {
        return percentage.ToString(percentage == decimal.Truncate(percentage) ? "0" : "0.##") + "%";
    }

}

public sealed class FacturaEmailNotificationDocument
{
    public Guid FacturaId { get; init; }
    public Guid EmpresaId { get; init; }
    public string DestinatarioEmail { get; init; } = string.Empty;
    public string DestinatarioNombre { get; init; } = string.Empty;
    public string NumeroComprobante { get; init; } = string.Empty;
    public DateTimeOffset FechaEmision { get; init; }
    public decimal Total { get; init; }
    public string XmlAutorizado { get; init; } = string.Empty;
    public FacturaRideReportModel Ride { get; init; } = default!;
}
