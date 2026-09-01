namespace TestDeIa.Shared.Responses.Retenciones;

public sealed class RetencionSriTestResultResponse
{
    public string NombrePrueba { get; set; } = string.Empty;

    public bool Succeeded { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string? ClaveAcceso { get; set; }

    public string? Mensaje { get; set; }

    public string? PayloadResumen { get; set; }

    public IReadOnlyCollection<string> Errores { get; set; } = Array.Empty<string>();
}
