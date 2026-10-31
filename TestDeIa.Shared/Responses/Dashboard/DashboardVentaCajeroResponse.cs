namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardVentaCajeroResponse
{
    public Guid FacturaId { get; set; }
    public string NumeroComprobante { get; set; } = string.Empty;
    public DateTimeOffset FechaEmision { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string FormaPago { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
}
