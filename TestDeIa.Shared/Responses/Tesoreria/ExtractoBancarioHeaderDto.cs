namespace TestDeIa.Shared.Responses.Tesoreria;

public sealed class ExtractoBancarioHeaderDto
{
    public Guid Id { get; set; }
    public Guid CuentaBancariaId { get; set; }
    public string BancoNombre { get; set; } = string.Empty;
    public string NumeroCuenta { get; set; } = string.Empty;
    public DateTime FechaImportacion { get; set; }
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public string NombreArchivoOriginal { get; set; } = string.Empty;
    public int TotalRegistros { get; set; }
    public string? Observaciones { get; set; }
}
