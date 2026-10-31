namespace TestDeIa.Shared.Requests.Integraciones;

public sealed class WhatsAppEnvioResponseDto
{
    public Guid LogId { get; set; }

    public bool Succeeded { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string? ProviderMessageId { get; set; }

    public string? Mensaje { get; set; }
}
