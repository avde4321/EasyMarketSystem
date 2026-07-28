namespace TestDeIa.Shared.Security;

public static class SecurityUserEstados
{
    public const string Activo = "Activo";
    public const string Inactivo = "Inactivo";
    public const string Bloqueado = "Bloqueado";

    public static string Normalize(string? value)
    {
        return value?.Trim().ToUpperInvariant() switch
        {
            "ACTIVO" => Activo,
            "INACTIVO" => Inactivo,
            "BLOQUEADO" => Bloqueado,
            _ => string.Empty
        };
    }
}
