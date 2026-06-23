using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Security;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contrasena es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}
