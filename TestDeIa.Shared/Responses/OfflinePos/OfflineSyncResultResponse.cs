namespace TestDeIa.Shared.Responses.OfflinePos;

public sealed class OfflineSyncResultResponse
{
    public int Recibidas { get; set; }

    public int Procesadas { get; set; }

    public int ConflictosStock { get; set; }

    public int ErroresValidacion { get; set; }

    public IReadOnlyCollection<OfflineSyncItemResultResponse> Resultados { get; set; } = Array.Empty<OfflineSyncItemResultResponse>();
}

public sealed class OfflineSyncItemResultResponse
{
    public Guid LocalQueueId { get; set; }

    public bool Succeeded { get; set; }

    public Guid? FacturaId { get; set; }

    public string? NumeroComprobante { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string? Mensaje { get; set; }
}
