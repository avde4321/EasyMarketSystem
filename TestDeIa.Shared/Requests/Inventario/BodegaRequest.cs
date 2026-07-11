using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class BodegaRequest
{
    [Required(ErrorMessage = "El nombre de la bodega es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre de la bodega no puede superar 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La direccion no puede superar 250 caracteres.")]
    public string? Direccion { get; set; }

    public bool IsActive { get; set; } = true;
}
