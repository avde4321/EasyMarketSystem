namespace TestDeIa.Shared.Responses.Catalogos;

public sealed class CatalogoItemResponse
{
    public Guid Id { get; set; }

    public Guid CatalogoId { get; set; }

    public string CatalogoCodigo { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int Orden { get; set; }

    public bool IsActive { get; set; }
}
