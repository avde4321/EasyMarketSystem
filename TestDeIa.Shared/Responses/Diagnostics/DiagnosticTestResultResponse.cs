namespace TestDeIa.Shared.Responses.Diagnostics;

public sealed class DiagnosticTestResultResponse
{
    public string NombrePrueba { get; set; } = string.Empty;

    public bool Succeeded { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public TimeSpan Duracion { get; set; }

    public IReadOnlyCollection<string> Errores { get; set; } = Array.Empty<string>();

    public IReadOnlyDictionary<string, string> Evidencias { get; set; } = new Dictionary<string, string>();
}

public sealed class DiagnosticSuiteResultResponse
{
    public string Suite { get; set; } = string.Empty;

    public DateTimeOffset EjecutadoEn { get; set; } = DateTimeOffset.UtcNow;

    public bool Succeeded => Resultados.All(current => current.Succeeded);

    public IReadOnlyCollection<DiagnosticTestResultResponse> Resultados { get; set; } = Array.Empty<DiagnosticTestResultResponse>();
}
