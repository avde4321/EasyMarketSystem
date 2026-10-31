using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class TransferenciaInventarioDetalleRequest
{
    [Required]
    public Guid ProductoId { get; set; }

    [Range(0.0001, 999999999)]
    public decimal Cantidad { get; set; }
}
