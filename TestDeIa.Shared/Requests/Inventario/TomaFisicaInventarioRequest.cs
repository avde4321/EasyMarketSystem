using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class TomaFisicaInventarioRequest
{
    [Required(ErrorMessage = "La bodega es obligatoria.")]
    public Guid BodegaId { get; set; }

    [StringLength(120, ErrorMessage = "El concepto no puede superar 120 caracteres.")]
    public string Concepto { get; set; } = "Toma fisica";

    [Required(ErrorMessage = "Debes enviar al menos un item contado.")]
    public List<TomaFisicaInventarioItemRequest> Items { get; set; } = [];
}
