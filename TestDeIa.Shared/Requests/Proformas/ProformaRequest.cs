namespace TestDeIa.Shared.Requests.Proformas;

public sealed class ProformaRequest
{
    public Guid ClienteId { get; set; }

    public Guid UsuarioId { get; set; }

    public Guid BodegaId { get; set; }

    public string? Secuencial { get; set; }

    public DateTime FechaEmision { get; set; } = DateTime.Today;

    public DateTime? FechaVencimiento { get; set; }

    public int Estado { get; set; }

    public decimal SubtotalSinImpuestos { get; set; }

    public decimal SubtotalIVA { get; set; }

    public decimal DescuentoTotal { get; set; }

    public decimal Total { get; set; }

    public string? Observacion { get; set; }

    public IReadOnlyCollection<ProformaDetalleRequest> Detalles { get; set; } = Array.Empty<ProformaDetalleRequest>();
}
