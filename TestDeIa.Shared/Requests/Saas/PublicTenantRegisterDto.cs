using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Saas;

public sealed class PublicTenantRegisterDto
{
    [Required(ErrorMessage = "Ingresa el nombre completo del administrador."), StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;
    [Required(ErrorMessage = "Ingresa tu correo electrónico."), EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido."), StringLength(180)]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Ingresa una contraseña."), StringLength(128, MinimumLength = 12, ErrorMessage = "La contraseña debe tener entre 12 y 128 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).+$", ErrorMessage = "La contraseña debe incluir mayúscula, minúscula, número y símbolo.")]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "Ingresa un teléfono de contacto."), StringLength(40), Phone(ErrorMessage = "Ingresa un teléfono válido.")]
    public string Telefono { get; set; } = string.Empty;
    [Required(ErrorMessage = "Ingresa el RUC de la empresa."), RegularExpression(@"^[0-9]{13}$", ErrorMessage = "El RUC debe contener exactamente 13 dígitos.")]
    public string Ruc { get; set; } = string.Empty;
    [Required(ErrorMessage = "Ingresa la razón social."), StringLength(300)]
    public string RazonSocial { get; set; } = string.Empty;
    [StringLength(300)]
    public string? NombreComercial { get; set; }
    [Required(ErrorMessage = "Ingresa la dirección de la empresa."), StringLength(250)]
    public string Direccion { get; set; } = string.Empty;
    public bool ObligadoLlevarContabilidad { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un plan disponible.")]
    public int PlanId { get; set; }
    public bool EsDemo { get; set; }
}
