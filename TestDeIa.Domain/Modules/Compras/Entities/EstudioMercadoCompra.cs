namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class EstudioMercadoCompra
{
    public EstudioMercadoCompra(
        Guid id,
        Guid empresaId,
        int anio,
        int mes,
        string topProductosVendidosJson,
        string sugerenciasCompraJson,
        string analisisEstrategicoIA,
        DateTimeOffset createdAt,
        Guid usuarioCreacionId,
        DateTimeOffset? updatedAt,
        Guid? usuarioModificacionId,
        IReadOnlyCollection<EstudioMercadoTopProducto> topProductosVendidos,
        IReadOnlyCollection<EstudioMercadoSugerenciaCompra> sugerenciasCompra)
    {
        Id = id;
        EmpresaId = empresaId;
        Anio = anio;
        Mes = mes;
        TopProductosVendidosJson = topProductosVendidosJson;
        SugerenciasCompraJson = sugerenciasCompraJson;
        AnalisisEstrategicoIA = analisisEstrategicoIA;
        CreatedAt = createdAt;
        UsuarioCreacionId = usuarioCreacionId;
        UpdatedAt = updatedAt;
        UsuarioModificacionId = usuarioModificacionId;
        TopProductosVendidos = topProductosVendidos;
        SugerenciasCompra = sugerenciasCompra;
    }

    public Guid Id { get; }
    public Guid EmpresaId { get; }
    public int Anio { get; }
    public int Mes { get; }
    public string TopProductosVendidosJson { get; }
    public string SugerenciasCompraJson { get; }
    public string AnalisisEstrategicoIA { get; }
    public DateTimeOffset CreatedAt { get; }
    public Guid UsuarioCreacionId { get; }
    public DateTimeOffset? UpdatedAt { get; }
    public Guid? UsuarioModificacionId { get; }
    public IReadOnlyCollection<EstudioMercadoTopProducto> TopProductosVendidos { get; }
    public IReadOnlyCollection<EstudioMercadoSugerenciaCompra> SugerenciasCompra { get; }
}
