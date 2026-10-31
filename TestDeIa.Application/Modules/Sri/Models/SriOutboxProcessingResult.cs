namespace TestDeIa.Application.Modules.Sri.Models;

public sealed class SriOutboxProcessingResult
{
    public bool Succeeded { get; init; }

    public bool Devuelto { get; init; }

    public string Message { get; init; } = string.Empty;
}
