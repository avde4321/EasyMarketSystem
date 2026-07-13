using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Empresa;

public sealed class EmpresaPuntoEmisionRequest
{
    public Guid? Id { get; set; }
    public Guid? BodegaId { get; set; }

    [StringLength(300, ErrorMessage = "La direccion del establecimiento no puede superar 300 caracteres.")]
    public string? DireccionEstablecimiento { get; set; }

    [Required(ErrorMessage = "El establecimiento es obligatorio.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "El establecimiento debe tener 3 digitos.")]
    public string Establecimiento { get; set; } = "001";

    [Required(ErrorMessage = "El punto de emision es obligatorio.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "El punto de emision debe tener 3 digitos.")]
    public string PuntoEmision { get; set; } = "001";

    public bool IsDefault { get; set; }
}
