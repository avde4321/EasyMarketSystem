namespace TestDeIa.Shared.Responses.Inventario;

public sealed class ProductoResponse
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public Guid? CategoriaId { get; set; }

    public string UnidadMedida { get; set; } = string.Empty;

    public string NaturalezaItem { get; set; } = string.Empty;

    public string CodigoIva { get; set; } = string.Empty;

    public decimal PorcentajeIva { get; set; }

    public decimal PrecioVenta { get; set; }

    public decimal CostoReferencial { get; set; }

    public decimal StockActual { get; set; }

    public decimal? StockMinimo { get; set; }

    public bool ControlaStock { get; set; }

    public bool AplicaComision { get; set; }

    public string? TipoComision { get; set; }

    public decimal? ValorComision { get; set; }

    public bool TieneAlertaStockMinimo => ControlaStock && StockMinimo.HasValue && StockActual <= StockMinimo.Value;

    public decimal CostoPromedio { get; set; }

    public bool IsActive { get; set; }
}

