namespace TestDeIa.Domain.Modules.Inventario.Entities;

public sealed class KardexMovimiento
{
    public KardexMovimiento(
        Guid id,
        Guid productoId,
        string tipoMovimiento,
        string concepto,
        string? referencia,
        decimal cantidadEntrada,
        decimal cantidadSalida,
        decimal saldoCantidad,
        decimal costoUnitario,
        decimal costoPromedio,
        decimal saldoValor,
        DateTimeOffset fechaMovimiento)
    {
        Id = id;
        ProductoId = productoId;
        TipoMovimiento = tipoMovimiento;
        Concepto = concepto;
        Referencia = referencia;
        CantidadEntrada = cantidadEntrada;
        CantidadSalida = cantidadSalida;
        SaldoCantidad = saldoCantidad;
        CostoUnitario = costoUnitario;
        CostoPromedio = costoPromedio;
        SaldoValor = saldoValor;
        FechaMovimiento = fechaMovimiento;
    }

    public Guid Id { get; }

    public Guid ProductoId { get; }

    public string TipoMovimiento { get; }

    public string Concepto { get; }

    public string? Referencia { get; }

    public decimal CantidadEntrada { get; }

    public decimal CantidadSalida { get; }

    public decimal SaldoCantidad { get; }

    public decimal CostoUnitario { get; }

    public decimal CostoPromedio { get; }

    public decimal SaldoValor { get; }

    public DateTimeOffset FechaMovimiento { get; }
}
