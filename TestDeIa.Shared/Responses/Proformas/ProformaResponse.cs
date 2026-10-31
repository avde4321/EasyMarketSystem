namespace TestDeIa.Shared.Responses.Proformas;

public sealed class ProformaResponse
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ClienteId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public Guid UsuarioId { get; set; }

    public string UsuarioNombre { get; set; } = string.Empty;

    public Guid BodegaId { get; set; }

    public string BodegaNombre { get; set; } = string.Empty;

    public string Secuencial { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public int Estado { get; set; }

    public string EstadoNombre { get; set; } = string.Empty;

    public decimal SubtotalSinImpuestos { get; set; }

    public decimal SubtotalIVA { get; set; }

    public decimal DescuentoTotal { get; set; }

    public decimal Total { get; set; }

    public string? Observacion { get; set; }

    public Guid? FacturaId { get; set; }

    public IReadOnlyCollection<ProformaDetalleResponse> Detalles { get; set; } = Array.Empty<ProformaDetalleResponse>();
}
