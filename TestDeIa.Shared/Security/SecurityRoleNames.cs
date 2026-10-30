namespace TestDeIa.Shared.Security;

public static class SecurityRoleNames
{
    public const string Administrador = "Administrador";
    public const string SuperAdministrador = "SuperAdministrador";
    public const string AdministradorEmpresa = "AdministradorEmpresa";
    public const string AdministradoresEmpresa = Administrador + "," + AdministradorEmpresa + "," + SuperAdministrador;
    public const string Gerente = "Gerente";
    public const string Cajero = "Cajero";
    public const string AsesorComercial = "AsesorComercial";
    public const string Bodeguero = "Bodeguero";
    public const string Contador = "Contador";

    public static readonly IReadOnlyCollection<string> All =
    [
        Administrador,
        AdministradorEmpresa,
        Gerente,
        Cajero,
        AsesorComercial,
        Bodeguero,
        Contador
    ];
}
