using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Catalogos;

public sealed class CatalogoItemRequest
{
    [Required(ErrorMessage = "El catalogo es obligatorio.")]
    [StringLength(80, ErrorMessage = "El codigo del catalogo no puede superar 80 caracteres.")]
    public string CatalogoCodigo { get; set; } = string.Empty;

    public Guid? ParentItemId { get; set; }

    [Required(ErrorMessage = "El codigo del item es obligatorio.")]
    [StringLength(80, ErrorMessage = "El codigo del item no puede superar 80 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre no puede superar 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripcion no puede superar 250 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0, 9999, ErrorMessage = "El orden debe estar entre 0 y 9999.")]
    public int Orden { get; set; }

    public bool IsActive { get; set; } = true;
}
