namespace TestDeIa.Shared.Responses.Inventario;

public sealed class KardexMovimientoResponse
{
    public Guid Id { get; set; }

    public Guid ProductoId { get; set; }

    public string TipoMovimiento { get; set; } = string.Empty;

    public string Concepto { get; set; } = string.Empty;

    public string? Referencia { get; set; }

    public decimal CantidadEntrada { get; set; }

    public decimal CantidadSalida { get; set; }

    public decimal SaldoCantidad { get; set; }

    public decimal CostoUnitario { get; set; }

    public decimal CostoPromedio { get; set; }

    public decimal SaldoValor { get; set; }

    public DateTimeOffset FechaMovimiento { get; set; }
}
