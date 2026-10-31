namespace TestDeIa.Shared.Responses.Geografia;

public sealed class GeoItemResponse
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
}
