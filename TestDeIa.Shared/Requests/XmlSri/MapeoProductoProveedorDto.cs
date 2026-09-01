namespace TestDeIa.Shared.Requests.XmlSri;

public sealed class MapeoProductoProveedorDto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ProveedorId { get; set; }

    public string CodigoProductoProveedor { get; set; } = string.Empty;

    public string? NombreProductoProveedor { get; set; }

    public Guid ProductoId { get; set; }

    public string ProductoCodigoInterno { get; set; } = string.Empty;

    public string ProductoNombreInterno { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
