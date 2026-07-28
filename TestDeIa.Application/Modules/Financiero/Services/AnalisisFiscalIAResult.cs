namespace TestDeIa.Application.Modules.Financiero.Services;

public sealed class AnalisisFiscalIAResult
{
    public string RazonamientoIA { get; init; } = string.Empty;
    public string ContextoPrevioUtilizado { get; init; } = string.Empty;
    public bool TieneAprendizajeAcumulado { get; init; }
}
