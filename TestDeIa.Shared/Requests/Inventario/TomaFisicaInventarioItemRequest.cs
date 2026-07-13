using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class TomaFisicaInventarioItemRequest
{
    [Required(ErrorMessage = "El producto es obligatorio.")]
    public Guid ProductoId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "La cantidad contada no puede ser negativa.")]
    public decimal CantidadContada { get; set; }
}
