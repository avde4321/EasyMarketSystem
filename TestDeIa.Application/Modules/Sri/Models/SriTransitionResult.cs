namespace TestDeIa.Application.Modules.Sri.Models;

public sealed class SriTransitionResult
{
    public bool Succeeded { get; init; }

    public string EstadoAnterior { get; init; } = string.Empty;

    public string EstadoNuevo { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
