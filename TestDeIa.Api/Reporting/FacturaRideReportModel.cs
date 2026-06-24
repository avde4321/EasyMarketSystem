namespace TestDeIa.Api.Reporting;

public sealed class FacturaRideReportModel
{
    public Guid FacturaId { get; init; }
    public string BannerImagePath { get; init; } = string.Empty;
    public string NumeroComprobante { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string ClaveAcceso { get; init; } = string.Empty;
    public string NumeroAutorizacion { get; init; } = "-";
    public string FechaEmision { get; init; } = string.Empty;
    public string FechaAutorizacion { get; init; } = "-";
    public string EmisorRazonSocial { get; init; } = string.Empty;
    public string EmisorNombreComercial { get; init; } = string.Empty;
    public string EmisorRuc { get; init; } = string.Empty;
    public string EmisorDireccionMatriz { get; init; } = string.Empty;
    public string EmisorDireccionSucursal { get; init; } = string.Empty;
    public string AmbienteSri { get; init; } = string.Empty;
    public string EmisionTipo { get; init; } = string.Empty;
    public string ObligadoContabilidad { get; init; } = "NO";
    public string ContribuyenteEspecial { get; init; } = string.Empty;
    public string ClienteNombre { get; init; } = string.Empty;
    public string ClienteIdentificacion { get; init; } = string.Empty;
    public string ClienteDireccion { get; init; } = "-";
    public string ClienteEmail { get; init; } = "-";
    public string ClienteTelefono { get; init; } = "-";
    public string FormaPago { get; init; } = string.Empty;
    public string GuiaRemision { get; init; } = string.Empty;
    public string Observacion { get; init; } = "-";
    public string WatermarkText { get; init; } = string.Empty;
    public string XmlGenerado { get; init; } = string.Empty;
    public string? XmlFirmado { get; init; }
    public decimal Subtotal { get; init; }
    public decimal IvaTotal { get; init; }
    public decimal Total { get; init; }
    public decimal SubtotalIva12 { get; init; }
    public decimal SubtotalIva0 { get; init; }
    public decimal SubtotalNoObjeto { get; init; }
    public decimal SubtotalExento { get; init; }
    public decimal SubtotalSinImpuestos { get; init; }
    public decimal Ice { get; init; }
    public decimal Irbpnr { get; init; }
    public decimal Propina { get; init; }
    public decimal Descuento { get; init; }
    public decimal TotalSubsidio { get; init; }
    public decimal TotalSinSubsidio { get; init; }
    public IReadOnlyCollection<FacturaRideDetalleRow> Detalles { get; init; } = [];
    public IReadOnlyCollection<FacturaRideTotalRow> Totales { get; init; } = [];
}

public sealed class FacturaRideDetalleRow
{
    public string CodigoPrincipal { get; init; } = string.Empty;
    public string CodigoAuxiliar { get; init; } = string.Empty;
    public string Cantidad { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string DetalleAdicional1 { get; init; } = string.Empty;
    public string DetalleAdicional2 { get; init; } = string.Empty;
    public string DetalleAdicional3 { get; init; } = string.Empty;
    public string PrecioUnitario { get; init; } = string.Empty;
    public string Subsidio { get; init; } = "0.00";
    public string PrecioSinSubsidio { get; init; } = "0.00";
    public string Descuento { get; init; } = "0.00";
    public string PrecioTotal { get; init; } = string.Empty;
}

public sealed class FacturaRideTotalRow
{
    public string Label { get; init; } = string.Empty;
    public string Valor { get; init; } = string.Empty;
}
