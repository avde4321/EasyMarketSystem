using System.Security.Cryptography.X509Certificates;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public static class SriCertificateSelector
{
    public static (byte[] Contenido, string Clave) Resolve(EmpresaEmisoraEntity empresa)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var candidate in empresa.CertificadosDigitales
            .Where(c => c.EmpresaId == empresa.Id && c.IsActive && c.FechaInicioVigencia <= now && c.FechaFinVigencia >= now)
            .OrderByDescending(c => c.EsPrincipal).ThenByDescending(c => c.FechaFinVigencia))
        {
            using var certificate = X509CertificateLoader.LoadPkcs12(candidate.Contenido, candidate.Clave, X509KeyStorageFlags.EphemeralKeySet);
            Validate(certificate);
            return (candidate.Contenido, candidate.Clave);
        }

        throw new InvalidOperationException("La empresa no tiene un certificado activo y vigente. Carga o activa una firma válida antes de emitir.");
    }

    public static void Validate(X509Certificate2 certificate)
    {
        var now = DateTime.UtcNow;
        if (!certificate.HasPrivateKey)
            throw new InvalidOperationException("El certificado no contiene una clave privada para firmar.");
        if (now < certificate.NotBefore.ToUniversalTime())
            throw new InvalidOperationException("El certificado todavía no está vigente.");
        if (now > certificate.NotAfter.ToUniversalTime())
            throw new InvalidOperationException("El certificado ha caducado. Debes renovar la firma electrónica.");
    }
}
