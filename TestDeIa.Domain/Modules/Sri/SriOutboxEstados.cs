namespace TestDeIa.Domain.Modules.Sri;

public static class SriOutboxEstados
{
    public const string Pendiente = "PENDIENTE";
    public const string Generado = SriEstadosComprobante.Generado;
    public const string Firmado = SriEstadosComprobante.Firmado;
    public const string EnProceso = "EN_PROCESO";
    public const string Autorizado = "AUTORIZADO";
    public const string NoAutorizado = SriEstadosComprobante.NoAutorizado;
    public const string Anulado = SriEstadosComprobante.Anulado;
    public const string Error = "ERROR";
    public const string Devuelto = "DEVUELTO";
    public const string Devuelta = SriEstadosComprobante.Devuelta;

    public static readonly IReadOnlySet<string> ValidStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pendiente,
        Generado,
        Firmado,
        EnProceso,
        Autorizado,
        NoAutorizado,
        Anulado,
        Error,
        Devuelto,
        Devuelta
    };
}
