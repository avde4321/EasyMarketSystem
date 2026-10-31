namespace TestDeIa.Shared.Sri;

public static class SriCatalogCodes
{
    public const string AmbientePruebas = "1";
    public const string AmbienteProduccion = "2";

    public const string TipoEmisionNormal = "1";

    public const string IdentificacionRuc = "04";
    public const string IdentificacionCedula = "05";
    public const string IdentificacionPasaporte = "06";
    public const string IdentificacionConsumidorFinal = "07";
    public const string IdentificacionExterior = "08";
    public const string IdentificacionPlaca = "09";

    public const string FormaPagoEfectivo = "01";
    public const string FormaPagoCompensacion = "15";
    public const string FormaPagoTarjetaDebito = "16";
    public const string FormaPagoDineroElectronico = "17";
    public const string FormaPagoTarjetaPrepago = "18";
    public const string FormaPagoTarjetaCredito = "19";
    public const string FormaPagoTransferencia = "20";
    public const string FormaPagoEndosoTitulos = "21";

    public static string? NormalizeAmbienteCode(string? value)
    {
        return Normalize(value) switch
        {
            AmbientePruebas or "PRUEBAS" => AmbientePruebas,
            AmbienteProduccion or "PRODUCCION" => AmbienteProduccion,
            _ => null
        };
    }

    public static string GetAmbienteName(string? value)
    {
        return NormalizeAmbienteCode(value) switch
        {
            AmbientePruebas => "Pruebas",
            AmbienteProduccion => "Produccion",
            _ => value?.Trim() ?? string.Empty
        };
    }

    public static bool IsProductionEnvironment(string? value) =>
        string.Equals(NormalizeAmbienteCode(value), AmbienteProduccion, StringComparison.Ordinal);

    public static string? NormalizeTipoEmisionCode(string? value)
    {
        return Normalize(value) switch
        {
            TipoEmisionNormal or "NORMAL" => TipoEmisionNormal,
            _ => null
        };
    }

    public static string GetTipoEmisionName(string? value)
    {
        return NormalizeTipoEmisionCode(value) switch
        {
            TipoEmisionNormal => "Normal",
            _ => value?.Trim() ?? string.Empty
        };
    }

    public static string? NormalizeTipoIdentificacionCode(string? value)
    {
        return Normalize(value) switch
        {
            IdentificacionRuc or "RUC" => IdentificacionRuc,
            IdentificacionCedula or "CEDULA" => IdentificacionCedula,
            IdentificacionPasaporte or "PASAPORTE" => IdentificacionPasaporte,
            IdentificacionConsumidorFinal or "CONSUMIDOR FINAL" => IdentificacionConsumidorFinal,
            IdentificacionExterior or "IDENTIFICACION DEL EXTERIOR" => IdentificacionExterior,
            IdentificacionPlaca or "PLACA" => IdentificacionPlaca,
            _ => null
        };
    }

    public static string GetTipoIdentificacionName(string? value)
    {
        return NormalizeTipoIdentificacionCode(value) switch
        {
            IdentificacionRuc => "RUC",
            IdentificacionCedula => "Cedula",
            IdentificacionPasaporte => "Pasaporte",
            IdentificacionConsumidorFinal => "Consumidor final",
            IdentificacionExterior => "Identificacion del exterior",
            IdentificacionPlaca => "Placa",
            _ => value?.Trim() ?? string.Empty
        };
    }

    public static string? NormalizeFormaPagoCode(string? value)
    {
        return Normalize(value) switch
        {
            FormaPagoEfectivo or "EFECTIVO" => FormaPagoEfectivo,
            FormaPagoCompensacion or "COMPENSACION" => FormaPagoCompensacion,
            FormaPagoTarjetaDebito or "TARJETA DE DEBITO" => FormaPagoTarjetaDebito,
            FormaPagoDineroElectronico or "DINERO ELECTRONICO" => FormaPagoDineroElectronico,
            FormaPagoTarjetaPrepago or "TARJETA PREPAGO" => FormaPagoTarjetaPrepago,
            FormaPagoTarjetaCredito or "TARJETA" or "TARJETA DE CREDITO" => FormaPagoTarjetaCredito,
            FormaPagoTransferencia or "TRANSFERENCIA" or "OTROS CON UTILIZACION DEL SISTEMA FINANCIERO" => FormaPagoTransferencia,
            FormaPagoEndosoTitulos or "ENDOSO DE TITULOS" => FormaPagoEndosoTitulos,
            _ => null
        };
    }

    public static string GetFormaPagoName(string? value)
    {
        return NormalizeFormaPagoCode(value) switch
        {
            FormaPagoEfectivo => "Efectivo",
            FormaPagoCompensacion => "Compensacion",
            FormaPagoTarjetaDebito => "Tarjeta de debito",
            FormaPagoDineroElectronico => "Dinero electronico",
            FormaPagoTarjetaPrepago => "Tarjeta prepago",
            FormaPagoTarjetaCredito => "Tarjeta de credito",
            FormaPagoTransferencia => "Transferencia",
            FormaPagoEndosoTitulos => "Endoso de titulos",
            _ => value?.Trim() ?? string.Empty
        };
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value
            .Trim()
            .ToUpperInvariant()
            .Replace("Á", "A", StringComparison.Ordinal)
            .Replace("É", "E", StringComparison.Ordinal)
            .Replace("Í", "I", StringComparison.Ordinal)
            .Replace("Ó", "O", StringComparison.Ordinal)
            .Replace("Ú", "U", StringComparison.Ordinal);
    }
}
