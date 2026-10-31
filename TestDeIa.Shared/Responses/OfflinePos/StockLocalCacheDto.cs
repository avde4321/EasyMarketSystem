namespace TestDeIa.Shared.Responses.OfflinePos;

public sealed class StockLocalCacheDto
{
    public Guid EmpresaId { get; set; }

    public Guid BodegaId { get; set; }

    public Guid ProductoId { get; set; }

    public string CodigoBarra { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public decimal TarifaIVA { get; set; }

    public decimal StockDisponible { get; set; }

    public bool ControlaStock { get; set; }

    public DateTimeOffset CachedAt { get; set; } = DateTimeOffset.Now;
}
