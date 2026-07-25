using Microsoft.Extensions.Options;
using TestDeIa.Infrastructure.Options;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SriUrlResolverService(IOptions<SriSoapOptions> options)
{
    private readonly SriSoapOptions options = options.Value;

    public SriEndpointSet Resolve(string ambienteSri)
    {
        return Resolve(ClaveAccesoService.GetTipoAmbiente(ambienteSri));
    }

    public SriEndpointSet Resolve(TipoAmbienteSri ambienteSri)
    {
        var environmentOptions = ambienteSri switch
        {
            TipoAmbienteSri.Pruebas => options.Pruebas,
            TipoAmbienteSri.Produccion => options.Produccion,
            _ => throw new InvalidOperationException("El ambiente SRI configurado para el SOAP no es valido.")
        };

        EnsureEndpoint(environmentOptions.RecepcionUrl, "recepcion");
        EnsureEndpoint(environmentOptions.AutorizacionUrl, "autorizacion");

        return new SriEndpointSet(environmentOptions.RecepcionUrl, environmentOptions.AutorizacionUrl);
    }

    private static void EnsureEndpoint(string endpoint, string serviceName)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException($"No se ha configurado la URL del servicio de {serviceName} del SRI.");
        }
    }
}

public sealed record SriEndpointSet(string RecepcionUrl, string AutorizacionUrl);
