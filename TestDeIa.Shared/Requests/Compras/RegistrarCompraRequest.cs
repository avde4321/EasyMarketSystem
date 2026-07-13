using System.ComponentModel.DataAnnotations;
using TestDeIa.Shared.Compras;

namespace TestDeIa.Shared.Requests.Compras;

public sealed class RegistrarCompraRequest
{
    [Required]
    public Guid ProveedorId { get; set; }

    [Required]
    public Guid BodegaId { get; set; }

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string TipoDocumentoCodigo { get; set; } = CompraDocumentTypes.FacturaProveedor;

    [RegularExpression(@"^\d{3}-\d{3}-\d{9}$", ErrorMessage = "El numero de comprobante debe tener el formato 001-001-000000001.")]
    public string? NumeroComprobante { get; set; } = "001-001-000000001";

    [StringLength(49, ErrorMessage = "La clave de acceso del proveedor no puede superar 49 caracteres.")]
    public string? ClaveAccesoProveedor { get; set; }

    [StringLength(3)]
    public string? Establecimiento { get; set; }

    [StringLength(3)]
    public string? PuntoEmision { get; set; }

    [StringLength(2)]
    public string FormaPago { get; set; } = "01";

    [StringLength(500)]
    public string? Observacion { get; set; }

    [Range(0, 3650, ErrorMessage = "Los dias de credito deben estar entre 0 y 3650.")]
    public int DiasCredito { get; set; }

    public DateTimeOffset FechaEmision { get; set; } = DateTimeOffset.Now;

    [MinLength(1, ErrorMessage = "Debes registrar al menos un producto.")]
    public List<RegistrarCompraDetalleRequest> Detalles { get; set; } = [];
}
