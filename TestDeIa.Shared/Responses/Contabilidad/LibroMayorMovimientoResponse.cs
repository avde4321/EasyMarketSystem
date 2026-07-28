namespace TestDeIa.Shared.Responses.Contabilidad;

public sealed class LibroMayorMovimientoResponse
{
    public DateTime FechaContable { get; set; }
    public string NumeroAsiento { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public string? DocumentoSoporte { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public decimal SaldoAcumulado { get; set; }
}
