namespace TestDeIa.Shared.Clientes;

public static class EstadoCreditoClienteCodes
{
    public const string AlDia = "AlDia";
    public const string Suspendido = "Suspendido";
    public const string Bloqueado = "Bloqueado";
    public static string? Normalize(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "NORMAL" or "ALDIA" => AlDia, // Compatibilidad con registros y clientes antiguos.
        "SUSPENDIDO" => Suspendido,
        "BLOQUEADO" => Bloqueado,
        _ => null
    };
}
