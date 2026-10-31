namespace TestDeIa.Shared.Responses.Catalogos;

public sealed class CatalogoResponse
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool IsActive { get; set; }

    public IReadOnlyCollection<CatalogoItemResponse> Items { get; set; } = [];
}
