namespace TestDeIa.Shared.Clientes;

public static class TipoClienteCodes
{
    public const string Natural = "Natural";
    public const string Juridico = "Juridico";

    public static string? Normalize(string? value)
        => string.Equals(value?.Trim(), Natural, StringComparison.OrdinalIgnoreCase) ? Natural
        : string.Equals(value?.Trim(), Juridico, StringComparison.OrdinalIgnoreCase) ? Juridico : null;
}
