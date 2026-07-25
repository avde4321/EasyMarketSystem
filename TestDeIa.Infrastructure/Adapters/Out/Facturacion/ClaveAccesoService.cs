using TestDeIa.Shared.Sri;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class ClaveAccesoService
{
    public string Generar(
        DateTimeOffset fechaEmision,
        string codigoDocumento,
        string ruc,
        string ambienteSri,
        string establecimiento,
        string puntoEmision,
        string secuencial,
        string codigoNumerico,
        string tipoEmision)
    {
        var ambienteCode = GetAmbienteCode(ambienteSri);
        var tipoEmisionCode = GetTipoEmisionCode(tipoEmision);

        var claveSinDigito =
            $"{fechaEmision:ddMMyyyy}" +
            codigoDocumento +
            ruc +
            ambienteCode +
            establecimiento +
            puntoEmision +
            secuencial.PadLeft(9, '0') +
            codigoNumerico +
            tipoEmisionCode;

        if (claveSinDigito.Length != 48 || !claveSinDigito.All(char.IsDigit))
        {
            throw new InvalidOperationException("La base de la clave de acceso SRI debe contener 48 digitos antes del digito verificador.");
        }

        return claveSinDigito + ComputeModulo11Digit(claveSinDigito);
    }

    public static string GetAmbienteCode(string ambienteSri)
    {
        return SriCatalogCodes.NormalizeAmbienteCode(ambienteSri)
            ?? throw new InvalidOperationException("El ambiente SRI configurado no es valido.");
    }

    public static TipoAmbienteSri GetTipoAmbiente(string ambienteSri)
    {
        return GetAmbienteCode(ambienteSri) switch
        {
            "1" => TipoAmbienteSri.Pruebas,
            "2" => TipoAmbienteSri.Produccion,
            _ => throw new InvalidOperationException("El ambiente SRI configurado no es valido.")
        };
    }

    public static string GetTipoEmisionCode(string tipoEmision)
    {
        return SriCatalogCodes.NormalizeTipoEmisionCode(tipoEmision)
            ?? throw new InvalidOperationException("El tipo de emision SRI configurado no es valido.");
    }

    private static int ComputeModulo11Digit(string key)
    {
        var factor = 2;
        var total = 0;

        for (var index = key.Length - 1; index >= 0; index--)
        {
            total += (key[index] - '0') * factor;
            factor++;

            if (factor > 7)
            {
                factor = 2;
            }
        }

        var modulo = 11 - (total % 11);

        return modulo switch
        {
            11 => 0,
            10 => 1,
            _ => modulo
        };
    }
}
