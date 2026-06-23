namespace TestDeIa.Application.Modules.Facturacion.Models;

public sealed class SriFacturaProcessingResult
{
    public string EstadoFinal { get; set; } = "Autorizado";

    public string ClaveAcceso { get; set; } = string.Empty;

    public string? NumeroAutorizacion { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public string XmlFirmado { get; set; } = string.Empty;

    public DateTimeOffset FechaRespuesta { get; set; }
}
