namespace TestDeIa.Application.Modules.Facturacion.Models;

public sealed class SriFacturaProcessingResult
{
    public string EstadoFinal { get; set; } = "AUTORIZADO";

    public string ClaveAcceso { get; set; } = string.Empty;

    public string? NumeroAutorizacion { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public string? XmlGenerado { get; set; }

    public string XmlFirmado { get; set; } = string.Empty;

    public DateTimeOffset FechaRespuesta { get; set; }
}
