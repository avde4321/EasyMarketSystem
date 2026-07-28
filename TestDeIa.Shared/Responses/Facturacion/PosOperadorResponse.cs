namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class PosOperadorResponse
{
    public Guid UsuarioId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string DisplayName => string.IsNullOrWhiteSpace(NombreCompleto) ? UserName : NombreCompleto;
}