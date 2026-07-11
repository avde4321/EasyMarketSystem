using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class EgresoMermaRequest
{
    [Required]
    public Guid ProductoId { get; set; }

    [Required]
    public Guid BodegaId { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public decimal Cantidad { get; set; }

    [Required(ErrorMessage = "El motivo de merma es obligatorio.")]
    [StringLength(160, ErrorMessage = "El motivo de merma no puede superar 160 caracteres.")]
    public string Motivo { get; set; } = "Merma";

    [StringLength(80, ErrorMessage = "La referencia no puede superar 80 caracteres.")]
    public string? Referencia { get; set; }
}
