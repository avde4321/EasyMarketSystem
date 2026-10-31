namespace TestDeIa.Shared.Responses.Reporteria;

public sealed class DashboardVentasDiariasResponse
{
    public DateTime Fecha { get; set; }
    public decimal VentaNeta { get; set; }
    public decimal IvaTotal { get; set; }
    public decimal TotalFacturado { get; set; }
    public int Comprobantes { get; set; }
}

public sealed class TopProductoRentabilidadResponse
{
    public Guid ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal CantidadVendida { get; set; }
    public decimal VentaNeta { get; set; }
    public decimal CostoHistorico { get; set; }
    public decimal UtilidadBruta { get; set; }
    public decimal MargenPorcentaje { get; set; }
}

public sealed class VentaBodegaMetodoPagoResponse
{
    public Guid BodegaId { get; set; }
    public string Bodega { get; set; } = string.Empty;
    public string FormaPagoCodigo { get; set; } = string.Empty;
    public string FormaPago { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Transacciones { get; set; }
}

public sealed class KpisRentabilidadResponse
{
    public decimal VentaNeta { get; set; }
    public decimal Devoluciones { get; set; }
    public decimal CostoVentas { get; set; }
    public decimal UtilidadBruta { get; set; }
    public decimal MargenPorcentaje { get; set; }
}

public sealed class ConsolidacionContableDiariaResponse
{
    public DateTime Fecha { get; set; }
    public decimal VentasNetas { get; set; }
    public decimal IvaVentas { get; set; }
    public decimal CostoVentas { get; set; }
    public decimal MovimientoInventario { get; set; }
    public decimal CajaBancos { get; set; }
    public decimal CuentasPorCobrar { get; set; }
    public decimal AjustesCaja { get; set; }
    public decimal MermasInventario { get; set; }
}

public sealed class KardexValorizadoReporteItemResponse
{
    public DateTimeOffset FechaMovimiento { get; set; }
    public Guid ProductoId { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public Guid BodegaId { get; set; }
    public string Bodega { get; set; } = string.Empty;
    public string TipoMovimiento { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public decimal CantidadEntrada { get; set; }
    public decimal CantidadSalida { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal CostoTotal { get; set; }
    public decimal SaldoCantidad { get; set; }
    public decimal SaldoValor { get; set; }
}

public sealed class CierreCajaReporteItemResponse
{
    public Guid CajaSesionId { get; set; }
    public DateTimeOffset FechaApertura { get; set; }
    public DateTimeOffset? FechaCierre { get; set; }
    public Guid UsuarioId { get; set; }
    public string EstadoCaja { get; set; } = string.Empty;
    public decimal MontoApertura { get; set; }
    public decimal MontoCalculadoTotal { get; set; }
    public decimal MontoDeclaradoTotal { get; set; }
    public decimal DiferenciaMonto { get; set; }
}
