namespace TestDeIa.Domain.Modules.Security.Entities;

public sealed class SecurityPermiso
{
    public SecurityPermiso(string id, string nombrePermiso, string descripcion, string modulo)
    {
        Id = id;
        NombrePermiso = nombrePermiso;
        Descripcion = descripcion;
        Modulo = modulo;
    }

    public string Id { get; }

    public string NombrePermiso { get; }

    public string Descripcion { get; }

    public string Modulo { get; }
}
