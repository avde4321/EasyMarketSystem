namespace TestDeIa.Domain.Modules.Sri;

public static class SriEstadosComprobante
{
    public const string Generado = "GENERADO";
    public const string Firmado = "FIRMADO";
    public const string Devuelta = "DEVUELTA";
    public const string EnProceso = "EN_PROCESO";
    public const string Autorizado = "AUTORIZADO";
    public const string NoAutorizado = "NO_AUTORIZADO";
    public const string Anulado = "ANULADO";

    public static readonly IReadOnlySet<string> EstadosFinales = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Autorizado,
        Anulado
    };

    public static readonly IReadOnlySet<string> ValidStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Generado,
        Firmado,
        Devuelta,
        EnProceso,
        Autorizado,
        NoAutorizado,
        Anulado
    };
}
