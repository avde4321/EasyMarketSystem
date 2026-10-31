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
        var codigoDocumentoNormalizado = NormalizeNumericSegment(codigoDocumento, 2, "tipo de comprobante");
        var rucNormalizado = NormalizeNumericSegment(ruc, 13, "RUC emisor");
        var establecimientoNormalizado = NormalizeNumericSegment(establecimiento, 3, "establecimiento");
        var puntoEmisionNormalizado = NormalizeNumericSegment(puntoEmision, 3, "punto de emision");
        var secuencialNormalizado = NormalizeNumericSegment(secuencial, 9, "secuencial", allowLeftPadding: true);
        var codigoNumericoNormalizado = NormalizeNumericSegment(codigoNumerico, 8, "codigo numerico", allowLeftPadding: true);

        var claveSinDigito =
            $"{fechaEmision:ddMMyyyy}" +
            codigoDocumentoNormalizado +
            rucNormalizado +
            ambienteCode +
            establecimientoNormalizado +
            puntoEmisionNormalizado +
            secuencialNormalizado +
            codigoNumericoNormalizado +
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

    private static string NormalizeNumericSegment(
        string value,
        int expectedLength,
        string fieldName,
        bool allowLeftPadding = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"La clave de acceso SRI requiere {fieldName}.");
        }

        var normalized = value.Trim();
        if (allowLeftPadding && normalized.Length < expectedLength)
        {
            normalized = normalized.PadLeft(expectedLength, '0');
        }

        if (normalized.Length != expectedLength || !normalized.All(char.IsDigit))
        {
            throw new InvalidOperationException($"El campo {fieldName} de la clave de acceso SRI debe contener {expectedLength} digitos numericos.");
        }

        return normalized;
    }
}
