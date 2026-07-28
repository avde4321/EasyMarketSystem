using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Security;

public sealed class ResetPasswordRequest
{
    [Required(ErrorMessage = "La clave temporal es obligatoria.")]
    [StringLength(80, MinimumLength = 6, ErrorMessage = "La clave temporal debe tener entre 6 y 80 caracteres.")]
    public string TemporaryPassword { get; set; } = string.Empty;
}
