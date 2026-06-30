namespace TestDeIa.Application.Modules.Facturacion.Models;

public sealed class SriFacturaProcessingResult
{
    public Domain.Modules.Facturacion.Entities.FacturaEstado EstadoFinal { get; set; } = Domain.Modules.Facturacion.Entities.FacturaEstado.AUTORIZADO;

    public string ClaveAcceso { get; set; } = string.Empty;

    public string? NumeroAutorizacion { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public string? XmlGenerado { get; set; }

    public string? XmlFirmado { get; set; }

    public DateTimeOffset FechaRespuesta { get; set; }
}
