namespace TestDeIa.Shared.Responses.Compras;

public sealed class ReporteComprasConsolidadoResponse
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string NaturalezaCompra { get; set; } = "Todos";
    public ReporteComprasDimensionFisicaResponse DimensionFisica { get; set; } = new();
    public ReporteComprasDimensionMonetariaResponse DimensionMonetaria { get; set; } = new();
}

public sealed class ReporteComprasDimensionFisicaResponse
{
    public decimal TotalUnidadesInventario { get; set; }
    public decimal TotalImporteInventario { get; set; }
    public int TotalActivosDadosAlta { get; set; }
    public decimal TotalImporteActivos { get; set; }
    public IReadOnlyCollection<ReporteComprasMovimientoFisicoResponse> Movimientos { get; set; } = Array.Empty<ReporteComprasMovimientoFisicoResponse>();
}

public sealed class ReporteComprasMovimientoFisicoResponse
{
    public DateTimeOffset Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string NaturalezaCompra { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public string ComprobanteSri { get; set; } = string.Empty;
    public string? ClaveAccesoSri { get; set; }
    public decimal Cantidad { get; set; }
    public decimal Importe { get; set; }
}

public sealed class ReporteComprasDimensionMonetariaResponse
{
    public decimal BaseIva0 { get; set; }
    public decimal BaseIva15 { get; set; }
    public decimal MontoIva { get; set; }
    public decimal TotalCompras { get; set; }
    public decimal SalidaEfectivaCajaBanco { get; set; }
    public decimal PasivoAcumuladoCxP { get; set; }
    public decimal TotalAbonosCxP { get; set; }
    public IReadOnlyCollection<ReporteComprasFlujoMonetarioResponse> Flujos { get; set; } = Array.Empty<ReporteComprasFlujoMonetarioResponse>();
}

public sealed class ReporteComprasFlujoMonetarioResponse
{
    public DateTimeOffset Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string FormaPago { get; set; } = string.Empty;
    public decimal TotalCompra { get; set; }
    public decimal Desembolso { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string? ComprobantePago { get; set; }
}
