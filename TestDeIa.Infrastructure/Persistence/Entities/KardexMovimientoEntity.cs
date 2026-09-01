namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class KardexMovimientoEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }

    public Guid ProductoId { get; set; }

    public ProductoEntity Producto { get; set; } = default!;

    public Guid BodegaId { get; set; }

    public BodegaEntity Bodega { get; set; } = default!;

    public string TipoMovimiento { get; set; } = string.Empty;

    public string Concepto { get; set; } = string.Empty;

    public string? Referencia { get; set; }

    public decimal CantidadEntrada { get; set; }

    public decimal CantidadSalida { get; set; }

    public decimal SaldoCantidad { get; set; }

    public decimal CostoUnitario { get; set; }

    public decimal CostoTotal { get; set; }

    public decimal CostoPromedio { get; set; }

    public decimal StockAnterior { get; set; }

    public decimal StockNuevo { get; set; }

    public decimal SaldoValor { get; set; }

    public Guid? FacturaId { get; set; }

    public Guid? CompraId { get; set; }

    public Guid? TransferenciaInventarioId { get; set; }

    public Guid? CreadoPorUsuarioId { get; set; }

    public DateTimeOffset FechaMovimiento { get; set; }
}
