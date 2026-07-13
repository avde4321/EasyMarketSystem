using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Compras;

public sealed class RegistrarCompraDetalleRequest
{
    [Required]
    public Guid ProductoId { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public decimal Cantidad { get; set; }

    [Range(0.000001, 999999999, ErrorMessage = "El costo unitario debe ser mayor a cero.")]
    public decimal CostoUnitario { get; set; }

    [Range(0, 999999999, ErrorMessage = "El descuento no puede ser negativo.")]
    public decimal Descuento { get; set; }
}
