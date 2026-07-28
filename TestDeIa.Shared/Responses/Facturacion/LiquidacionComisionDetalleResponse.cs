namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class LiquidacionComisionDetalleResponse
{
    public Guid FacturaId { get; set; }

    public Guid DetalleId { get; set; }

    public DateTimeOffset FechaEmision { get; set; }

    public string NumeroFactura { get; set; } = string.Empty;

    public Guid OperadorId { get; set; }

    public string Operador { get; set; } = string.Empty;

    public string IdentificacionOperador { get; set; } = string.Empty;

    public string Servicio { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal Subtotal { get; set; }

    public decimal MontoComision { get; set; }
}