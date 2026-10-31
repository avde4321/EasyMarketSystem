namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class PosClienteResponse
{
    public Guid? ClienteId { get; set; }

    public Guid PersonaId { get; set; }

    public string TipoIdentificacion { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string? NombreComercial { get; set; }

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool HasClienteExtension { get; set; }
}
