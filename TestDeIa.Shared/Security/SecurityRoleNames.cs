namespace TestDeIa.Shared.Security;

public static class SecurityRoleNames
{
    public const string Administrador = "Administrador";
    public const string Cajero = "Cajero";
    public const string Bodeguero = "Bodeguero";
    public const string Contador = "Contador";

    public static readonly IReadOnlyCollection<string> All =
    [
        Administrador,
        Cajero,
        Bodeguero,
        Contador
    ];
}
