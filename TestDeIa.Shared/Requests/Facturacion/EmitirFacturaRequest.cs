using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Facturacion;

public sealed class EmitirFacturaRequest
{
    [Required]
    public Guid ClienteId { get; set; }

    [Required(ErrorMessage = "La forma de pago es obligatoria.")]
    [StringLength(60, ErrorMessage = "La forma de pago no puede superar 60 caracteres.")]
    public string FormaPago { get; set; } = "Efectivo";

    [StringLength(300, ErrorMessage = "La observacion no puede superar 300 caracteres.")]
    public string? Observacion { get; set; }

    [Required]
    public IReadOnlyCollection<EmitirFacturaDetalleRequest> Items { get; set; } = [];
}

public sealed class EmitirFacturaDetalleRequest
{
    [Required]
    public Guid ProductoId { get; set; }

    [Range(0.0001, 999999999, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public decimal Cantidad { get; set; }
}
