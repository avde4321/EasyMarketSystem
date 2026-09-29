namespace TestDeIa.Shared.Responses.Tesoreria;

public sealed class ConciliacionMatchDto
{
    public Guid ExtractoDetalleId { get; set; }
    public Guid? MovimientoTesoreriaId { get; set; }
    public string NumeroDocumentoRef { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaExtracto { get; set; }
    public DateTime? FechaMovimiento { get; set; }
    public int Nivel { get; set; }
    public int Confianza { get; set; }
    public bool Conciliado { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
