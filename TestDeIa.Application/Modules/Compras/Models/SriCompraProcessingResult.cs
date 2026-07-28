using TestDeIa.Domain.Modules.Facturacion.Entities;

namespace TestDeIa.Application.Modules.Compras.Models;

public sealed class SriCompraProcessingResult
{
    public FacturaEstado EstadoFinal { get; set; } = FacturaEstado.PENDIENTE;
    public string ClaveAcceso { get; set; } = string.Empty;
    public string? NumeroAutorizacion { get; set; }
    public string? XmlGenerado { get; set; }
    public string? XmlFirmado { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTimeOffset FechaRespuesta { get; set; } = DateTimeOffset.UtcNow;
}
