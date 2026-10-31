using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Security;

public sealed class UpdateUserEstadoRequest
{
    [Required(ErrorMessage = "El estado del usuario es obligatorio.")]
    [StringLength(20, ErrorMessage = "El estado del usuario no puede superar 20 caracteres.")]
    public string Estado { get; set; } = string.Empty;
}
