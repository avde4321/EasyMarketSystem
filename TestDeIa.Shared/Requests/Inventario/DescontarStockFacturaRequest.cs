using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class DescontarStockFacturaRequest
{
    [Required]
    public string ReferenciaFactura { get; set; } = string.Empty;

    public Guid? BodegaId { get; set; }

    [Required]
    public IReadOnlyCollection<DescontarStockFacturaItemRequest> Items { get; set; } = [];
}

public sealed class DescontarStockFacturaItemRequest
{
    [Required]
    public Guid ProductoId { get; set; }

    [Range(0.01, 999999999)]
    public decimal Cantidad { get; set; }
}
