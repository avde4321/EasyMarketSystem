using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class RecepcionTransferenciaInventarioDetalleRequest
{
    [Required]
    public Guid DetalleId { get; set; }

    [Range(0, 999999999)]
    public decimal CantidadRecibida { get; set; }
}
