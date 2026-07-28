namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class EstudioMercadoCompraEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string TopProductosVendidosJson { get; set; } = string.Empty;
    public string SugerenciasCompraJson { get; set; } = string.Empty;
    public string AnalisisEstrategicoIA { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UsuarioCreacionId { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UsuarioModificacionId { get; set; }
}
