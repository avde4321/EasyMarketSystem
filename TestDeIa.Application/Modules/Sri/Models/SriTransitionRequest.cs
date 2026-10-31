namespace TestDeIa.Application.Modules.Sri.Models;

public sealed class SriTransitionRequest
{
    public Guid EmpresaId { get; init; }

    public Guid ComprobanteId { get; init; }

    public string TipoDocumentoId { get; init; } = string.Empty;

    public string EstadoNuevoCodigo { get; init; } = string.Empty;

    public string? CodigoErrorSri { get; init; }

    public string? MensajeRespuesta { get; init; }

    public Guid? UsuarioId { get; init; }

    public string? WorkerNode { get; init; }
}
