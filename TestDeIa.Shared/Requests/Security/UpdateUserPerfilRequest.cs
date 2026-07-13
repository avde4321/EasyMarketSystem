using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Security;

public sealed class UpdateUserPerfilRequest
{
    [Required(ErrorMessage = "Debes seleccionar al menos un rol para el usuario.")]
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}
