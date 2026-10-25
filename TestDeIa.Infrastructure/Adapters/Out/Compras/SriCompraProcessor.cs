using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Compras.Models;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Domain.Modules.Compras.Entities;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class SriCompraProcessor : ISriCompraProcessor
{
    private readonly TestDeIaDbContext dbContext;
    private readonly SriLiquidacionCompraXmlValidator validator;
    private readonly SriXadesBesSigner signer;

    public SriCompraProcessor(
        TestDeIaDbContext dbContext,
        SriLiquidacionCompraXmlValidator validator,
        SriXadesBesSigner signer)
    {
        this.dbContext = dbContext;
        this.validator = validator;
        this.signer = signer;
    }

    public async Task<SriCompraProcessingResult> ProcessAsync(Compra compra, CancellationToken cancellationToken = default)
    {
        var empresa = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .Include(current => current.CertificadosDigitales)
            .FirstOrDefaultAsync(current => current.Id == compra.EmpresaId && current.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la empresa emisora activa para firmar la liquidacion.");

        if (empresa.ModoDesarrollo)
        {
            return new SriCompraProcessingResult
            {
                EstadoFinal = FacturaEstado.AUTORIZADO,
                ClaveAcceso = compra.ClaveAccesoGenerada ?? string.Empty,
                NumeroAutorizacion = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}{compra.Secuencial}",
                XmlGenerado = compra.XmlGenerado,
                XmlFirmado = compra.XmlGenerado,
                Mensaje = "Liquidacion autorizada por la simulacion interna de desarrollo.",
                FechaRespuesta = DateTimeOffset.UtcNow
            };
        }

        if (string.IsNullOrWhiteSpace(compra.ClaveAccesoGenerada))
        {
            throw new InvalidOperationException("La liquidacion no tiene clave de acceso generada.");
        }

        if (string.IsNullOrWhiteSpace(compra.XmlGenerado))
        {
            throw new InvalidOperationException("La liquidacion no tiene XML generado.");
        }

        try
        {
            validator.Validate(compra.XmlGenerado);
        }
        catch (InvalidOperationException exception)
        {
            return BuildRejected(compra, exception.Message);
        }

        try
        {
            var certificado = SriCertificateSelector.Resolve(empresa);
            var xmlFirmado = signer.Sign(compra.XmlGenerado, certificado.Contenido, certificado.Clave);
            return new SriCompraProcessingResult
            {
                EstadoFinal = FacturaEstado.PENDIENTE,
                ClaveAcceso = compra.ClaveAccesoGenerada,
                XmlGenerado = compra.XmlGenerado,
                XmlFirmado = xmlFirmado,
                Mensaje = "XML firmado correctamente bajo XAdES-BES. La liquidacion queda pendiente de transmision/autorizacion.",
                FechaRespuesta = DateTimeOffset.UtcNow
            };
        }
        catch (CryptographicException exception)
        {
            return BuildUnsigned(compra, $"No se pudo firmar la liquidacion con el certificado .p12. Detalle: {exception.Message}");
        }
        catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException or FormatException)
        {
            return BuildUnsigned(compra, $"La firma XAdES-BES de la liquidacion no pudo generarse. Detalle: {exception.Message}");
        }
    }

    private static SriCompraProcessingResult BuildUnsigned(Compra compra, string mensaje)
    {
        return new SriCompraProcessingResult
        {
            EstadoFinal = FacturaEstado.NO_FIRMADO,
            ClaveAcceso = compra.ClaveAccesoGenerada ?? string.Empty,
            XmlGenerado = compra.XmlGenerado,
            Mensaje = mensaje,
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }

    private static SriCompraProcessingResult BuildRejected(Compra compra, string mensaje)
    {
        return new SriCompraProcessingResult
        {
            EstadoFinal = FacturaEstado.RECHAZADO,
            ClaveAcceso = compra.ClaveAccesoGenerada ?? string.Empty,
            XmlGenerado = compra.XmlGenerado,
            Mensaje = mensaje,
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }
}
