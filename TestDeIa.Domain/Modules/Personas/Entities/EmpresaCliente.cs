namespace TestDeIa.Domain.Modules.Personas.Entities;

public sealed class EmpresaCliente
{
    public Guid Id { get; init; }

    public Guid EmpresaId { get; init; }

    public string RazonSocial { get; init; } = string.Empty;

    public string? NombreComercial { get; init; }

    public string Ruc { get; init; } = string.Empty;

    public string? RepresentanteLegal { get; init; }

    public bool ObligadoLlevarContabilidad { get; init; }

    public string? ContribuyenteEspecial { get; init; }

    public string? EmailFacturacion { get; init; }

    public string? Telefono { get; init; }

    public string DireccionMatriz { get; init; } = string.Empty;
}
