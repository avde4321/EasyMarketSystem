namespace TestDeIa.Shared.Responses.Contabilidad;

public sealed class LibroMayorResponse
{
    public Guid CuentaContableId { get; set; }
    public string CuentaCodigo { get; set; } = string.Empty;
    public string CuentaNombre { get; set; } = string.Empty;
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public decimal SaldoFinal { get; set; }
    public IReadOnlyCollection<LibroMayorMovimientoResponse> Movimientos { get; set; } = [];
}
