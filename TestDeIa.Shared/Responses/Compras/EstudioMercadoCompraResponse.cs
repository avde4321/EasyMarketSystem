namespace TestDeIa.Shared.Responses.Compras;

public sealed class EstudioMercadoCompraResponse
{
    public Guid Id { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public IReadOnlyCollection<EstudioMercadoTopProductoResponse> TopProductosVendidos { get; set; } = Array.Empty<EstudioMercadoTopProductoResponse>();
    public IReadOnlyCollection<EstudioMercadoSugerenciaCompraResponse> SugerenciasCompra { get; set; } = Array.Empty<EstudioMercadoSugerenciaCompraResponse>();
    public string AnalisisEstrategicoIA { get; set; } = string.Empty;
}
