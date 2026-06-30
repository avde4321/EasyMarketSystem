using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Facturacion.Models;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SriFacturaProcessor : ISriFacturaProcessor
{
    private static readonly HashSet<decimal> SupportedIvaRates = [0m, 5m, 8m, 15m];

    private readonly TestDeIaDbContext dbContext;
    private readonly SriFacturaXmlSchemaValidator xmlSchemaValidator;
    private readonly SriXadesBesSigner signer;

    public SriFacturaProcessor(
        TestDeIaDbContext dbContext,
        SriFacturaXmlSchemaValidator xmlSchemaValidator,
        SriXadesBesSigner signer)
    {
        this.dbContext = dbContext;
        this.xmlSchemaValidator = xmlSchemaValidator;
        this.signer = signer;
    }

    public async Task<SriFacturaProcessingResult> ProcessAsync(Factura factura, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(factura.ClaveAcceso))
        {
            throw new InvalidOperationException("La factura no tiene una clave de acceso generada.");
        }

        if (string.IsNullOrWhiteSpace(factura.XmlGenerado))
        {
            throw new InvalidOperationException("La factura no tiene un XML base generado.");
        }

        if (factura.Detalles.Any(detalle => !SupportedIvaRates.Contains(detalle.PorcentajeIva)))
        {
            return BuildRejectedResult(
                factura,
                "El comprobante contiene una tarifa de IVA no soportada por el motor tributario actual.");
        }

        try
        {
            xmlSchemaValidator.Validate(factura.XmlGenerado);
        }
        catch (InvalidOperationException exception)
        {
            return BuildRejectedResult(factura, exception.Message);
        }

        var empresa = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == factura.EmpresaId && current.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la empresa emisora activa para firmar la factura.");

        if (empresa.CertificadoContenido is null || empresa.CertificadoContenido.Length == 0)
        {
            return BuildUnsignedResult(factura, "La empresa activa no tiene un certificado .p12 cargado para firmar el comprobante.");
        }

        if (string.IsNullOrWhiteSpace(empresa.CertificadoClave))
        {
            return BuildUnsignedResult(factura, "La empresa activa no tiene configurada la clave del certificado .p12.");
        }

        try
        {
            var xmlFirmado = signer.Sign(factura.XmlGenerado, empresa.CertificadoContenido, empresa.CertificadoClave);

            return new SriFacturaProcessingResult
            {
                EstadoFinal = FacturaEstado.PENDIENTE,
                ClaveAcceso = factura.ClaveAcceso,
                XmlGenerado = factura.XmlGenerado,
                XmlFirmado = xmlFirmado,
                Mensaje = "XML firmado correctamente bajo XAdES-BES. El comprobante queda pendiente de transmision/autorizacion.",
                FechaRespuesta = DateTimeOffset.UtcNow
            };
        }
        catch (CryptographicException exception)
        {
            return BuildUnsignedResult(factura, $"No se pudo firmar el comprobante con el certificado .p12. Detalle: {exception.Message}");
        }
        catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException or FormatException)
        {
            return BuildUnsignedResult(factura, $"La firma XAdES-BES no pudo generarse. Detalle: {exception.Message}");
        }
    }

    private static SriFacturaProcessingResult BuildUnsignedResult(Factura factura, string mensaje)
    {
        return new SriFacturaProcessingResult
        {
            EstadoFinal = FacturaEstado.NO_FIRMADO,
            ClaveAcceso = factura.ClaveAcceso ?? string.Empty,
            XmlGenerado = factura.XmlGenerado,
            Mensaje = mensaje,
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }

    private static SriFacturaProcessingResult BuildRejectedResult(Factura factura, string mensaje)
    {
        return new SriFacturaProcessingResult
        {
            EstadoFinal = FacturaEstado.RECHAZADO,
            ClaveAcceso = factura.ClaveAcceso ?? string.Empty,
            XmlGenerado = factura.XmlGenerado,
            Mensaje = mensaje,
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }
}
