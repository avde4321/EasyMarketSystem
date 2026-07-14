using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Facturacion;

public sealed class EmitirFacturaRequest
{
    [Required]
    public Guid ClienteId { get; set; }

    public Guid? BodegaId { get; set; }

    [Required(ErrorMessage = "El establecimiento es obligatorio.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "El establecimiento debe tener 3 digitos.")]
    public string Establecimiento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El punto de emision es obligatorio.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "El punto de emision debe tener 3 digitos.")]
    public string PuntoEmision { get; set; } = string.Empty;

    [Required(ErrorMessage = "La forma de pago es obligatoria.")]
    [StringLength(60, ErrorMessage = "La forma de pago no puede superar 60 caracteres.")]
    public string FormaPago { get; set; } = "01";

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

    [Range(0, 999999999, ErrorMessage = "El descuento no puede ser negativo.")]
    public decimal Descuento { get; set; }

    [Range(0, 999999999, ErrorMessage = "El precio unitario no puede ser negativo.")]
    public decimal? PrecioUnitarioOverride { get; set; }

    public Guid? UsuarioIdOperador { get; set; }
}