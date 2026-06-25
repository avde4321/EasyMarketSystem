using Microsoft.EntityFrameworkCore;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;

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
        var subtotalIva12 = factura.Detalles
            .Where(current => current.PorcentajeIva > 0)
            .Sum(current => current.Subtotal);

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

        var totalesRows = new[]
        {
            new FacturaRideTotalRow { Label = "SUBTOTAL 12%", Valor = subtotalIva12.ToString("0.00") },
            new FacturaRideTotalRow { Label = "SUBTOTAL IVA 0%", Valor = factura.SubtotalIva0.ToString("0.00") },
            new FacturaRideTotalRow { Label = "SUBTOTAL NO OBJETO IVA", Valor = "0.00" },
            new FacturaRideTotalRow { Label = "SUBTOTAL EXENTO IVA", Valor = "0.00" },
            new FacturaRideTotalRow { Label = "SUBTOTAL SIN IMPUESTOS", Valor = factura.Subtotal.ToString("0.00") },
            new FacturaRideTotalRow { Label = "DESCUENTO", Valor = factura.TotalDescuento.ToString("0.00") },
            new FacturaRideTotalRow { Label = "ICE", Valor = "0.00" },
            new FacturaRideTotalRow { Label = "IVA 12%", Valor = factura.IvaTotal.ToString("0.00") },
            new FacturaRideTotalRow { Label = "IRBPNR", Valor = "0.00" },
            new FacturaRideTotalRow { Label = "PROPINA", Valor = "0.00" },
            new FacturaRideTotalRow { Label = "VALOR TOTAL", Valor = factura.Total.ToString("0.00") }
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
            FechaAutorizacion = factura.FechaAutorizacion?.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") ?? "-",
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
            SubtotalIva12 = subtotalIva12,
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

    private static string ResolveWatermark(FacturaEstado estado)
    {
        return estado switch
        {
            FacturaEstado.NO_FIRMADO => "DOCUMENTO NO FIRMADO",
            FacturaEstado.PENDIENTE => "PENDIENTE DE AUTORIZACION",
            _ => string.Empty
        };
    }

}
