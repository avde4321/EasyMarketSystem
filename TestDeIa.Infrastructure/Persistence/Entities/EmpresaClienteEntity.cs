namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class EmpresaClienteEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string RazonSocial { get; set; } = string.Empty;

    public string? NombreComercial { get; set; }

    public string Ruc { get; set; } = string.Empty;

    public string? RepresentanteLegal { get; set; }

    public bool ObligadoLlevarContabilidad { get; set; }

    public string? ContribuyenteEspecial { get; set; }

    public string? EmailFacturacion { get; set; }

    public string? Telefono { get; set; }

    public string DireccionMatriz { get; set; } = string.Empty;
}
