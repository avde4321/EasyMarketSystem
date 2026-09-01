namespace TestDeIa.Domain.Modules.Sri;

public static class SriOutboxEstados
{
    public const string Pendiente = "PENDIENTE";
    public const string EnProceso = "EN_PROCESO";
    public const string Autorizado = "AUTORIZADO";
    public const string Error = "ERROR";
    public const string Devuelto = "DEVUELTO";

    public static readonly IReadOnlySet<string> ValidStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pendiente,
        EnProceso,
        Autorizado,
        Error,
        Devuelto
    };
}
