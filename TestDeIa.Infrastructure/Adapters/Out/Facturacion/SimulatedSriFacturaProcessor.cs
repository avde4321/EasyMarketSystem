using TestDeIa.Application.Modules.Facturacion.Models;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SimulatedSriFacturaProcessor : ISriFacturaProcessor
{
    private static readonly HashSet<decimal> SupportedIvaRates = [0m, 5m, 8m, 15m];

    public async Task<SriFacturaProcessingResult> ProcessAsync(Factura factura, CancellationToken cancellationToken = default)
    {
        await Task.Delay(900, cancellationToken);

        if (string.IsNullOrWhiteSpace(factura.ClaveAcceso))
        {
            throw new InvalidOperationException("La factura no tiene una clave de acceso generada.");
        }

        if (string.IsNullOrWhiteSpace(factura.XmlGenerado))
        {
            throw new InvalidOperationException("La factura no tiene un XML base generado.");
        }

        if (string.IsNullOrWhiteSpace(factura.XmlFirmado))
        {
            return new SriFacturaProcessingResult
            {
                EstadoFinal = FacturaEstados.NoFirmado,
                ClaveAcceso = factura.ClaveAcceso,
                XmlGenerado = factura.XmlGenerado,
                Mensaje = "El comprobante no pudo firmarse digitalmente. Revise el certificado .p12 configurado.",
                FechaRespuesta = DateTimeOffset.UtcNow
            };
        }

        if (factura.Detalles.Any(detalle => !SupportedIvaRates.Contains(detalle.PorcentajeIva)))
        {
            return new SriFacturaProcessingResult
            {
                EstadoFinal = FacturaEstados.Rechazado,
                ClaveAcceso = factura.ClaveAcceso,
                Mensaje = "El comprobante contiene una tarifa de IVA no soportada por el motor SRI.",
                XmlGenerado = factura.XmlGenerado,
                XmlFirmado = factura.XmlFirmado,
                FechaRespuesta = DateTimeOffset.UtcNow
            };
        }

        return new SriFacturaProcessingResult
        {
            EstadoFinal = FacturaEstados.Autorizado,
            ClaveAcceso = factura.ClaveAcceso,
            NumeroAutorizacion = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}{factura.Secuencial:000000000}",
            Mensaje = "Comprobante autorizado por el flujo asincrono.",
            XmlGenerado = factura.XmlGenerado,
            XmlFirmado = factura.XmlFirmado,
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }
}
