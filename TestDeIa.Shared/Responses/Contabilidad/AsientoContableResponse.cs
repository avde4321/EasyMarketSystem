namespace TestDeIa.Shared.Responses.Contabilidad;

public sealed class AsientoContableResponse
{
    public Guid Id { get; set; }
    public string NumeroAsiento { get; set; } = string.Empty;
    public DateTime FechaContable { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public string ModuloOrigen { get; set; } = string.Empty;
    public string? DocumentoSoporte { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public IReadOnlyCollection<AsientoDetalleResponse> Detalles { get; set; } = [];
}
