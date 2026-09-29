namespace TestDeIa.Shared.Responses.Tesoreria;

public sealed class ExtractoBancarioDetalleDto
{
    public Guid Id { get; set; }
    public Guid ExtractoHeaderId { get; set; }
    public DateTime FechaTransaccion { get; set; }
    public string NumeroDocumentoRef { get; set; } = string.Empty;
    public string ConceptoDescripcion { get; set; } = string.Empty;
    public byte TipoMovimiento { get; set; }
    public string TipoMovimientoNombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public bool Conciliado { get; set; }
    public Guid? MovimientoTesoreriaId { get; set; }
}
