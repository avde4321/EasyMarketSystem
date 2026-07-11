namespace TestDeIa.Shared.Responses.Inventario;

public sealed class BodegaResponse
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Direccion { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
