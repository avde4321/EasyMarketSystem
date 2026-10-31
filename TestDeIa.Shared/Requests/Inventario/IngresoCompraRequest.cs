using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class IngresoCompraRequest
{
    [Required]
    public Guid ProductoId { get; set; }

    [Required]
    public Guid BodegaId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "La cantidad debe ser mayor a cero.", ParseLimitsInInvariantCulture = true)]
    public decimal Cantidad { get; set; }

    [Range(typeof(decimal), "0.000001", "999999999", ErrorMessage = "El costo unitario de compra debe ser mayor a cero.", ParseLimitsInInvariantCulture = true)]
    public decimal CostoUnitarioCompra { get; set; }

    [StringLength(80, ErrorMessage = "La referencia no puede superar 80 caracteres.")]
    public string? Referencia { get; set; }
}
