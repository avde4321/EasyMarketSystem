namespace TestDeIa.Domain.Modules.Tesoreria.Entities;

public sealed class ExtractoBancarioHeader
{
    public Guid Id { get; init; }
    public Guid CuentaBancariaId { get; init; }
    public DateTime FechaImportacion { get; init; }
    public DateTime FechaDesde { get; init; }
    public DateTime FechaHasta { get; init; }
    public string NombreArchivoOriginal { get; init; } = string.Empty;
    public int TotalRegistros { get; init; }
    public string? Observaciones { get; init; }
}
