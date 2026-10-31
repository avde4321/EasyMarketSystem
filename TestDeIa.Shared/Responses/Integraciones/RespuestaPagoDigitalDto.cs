namespace TestDeIa.Shared.Responses.Integraciones;

public sealed class RespuestaPagoDigitalDto
{
    public string TransactionId { get; set; } = string.Empty;

    public string? LinkPagoUrl { get; set; }

    public string? QrCodeBase64 { get; set; }

    public string Estado { get; set; } = "Pendiente";
}
