namespace TestDeIa.Shared.Responses.Reporteria;

public sealed class ReporteVentasResponse
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public ReporteVentasResumenResponse Resumen { get; set; } = new();
    public IReadOnlyCollection<ReporteVentasPeriodoResponse> VentasMensuales { get; set; } = Array.Empty<ReporteVentasPeriodoResponse>();
    public IReadOnlyCollection<ReporteVentasProductoResponse> TopProductos { get; set; } = Array.Empty<ReporteVentasProductoResponse>();
    public IReadOnlyCollection<ReporteVentasVendedorResponse> ProductividadVendedores { get; set; } = Array.Empty<ReporteVentasVendedorResponse>();
    public IReadOnlyCollection<ReporteVentasProyeccionResponse> Proyecciones { get; set; } = Array.Empty<ReporteVentasProyeccionResponse>();
}

public sealed class ReporteVentasResumenResponse
{
    public decimal TotalVentas { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Descuentos { get; set; }
    public int Comprobantes { get; set; }
    public decimal TicketPromedio { get; set; }
    public decimal CostoEstimado { get; set; }
    public decimal MargenEstimado { get; set; }
    public decimal PromedioDiario { get; set; }
}

public sealed class ReporteVentasPeriodoResponse
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public decimal TotalVentas { get; set; }
    public int Comprobantes { get; set; }
}

public sealed class ReporteVentasProductoResponse
{
    public Guid ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal Total { get; set; }
    public decimal MargenEstimado { get; set; }
}

public sealed class ReporteVentasVendedorResponse
{
    public Guid UsuarioId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public int Comprobantes { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TicketPromedio { get; set; }
}

public sealed class ReporteVentasProyeccionResponse
{
    public string Horizonte { get; set; } = string.Empty;
    public int DiasEstimados { get; set; }
    public decimal TotalProyectado { get; set; }
    public decimal MargenProyectado { get; set; }
}
