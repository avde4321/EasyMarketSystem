using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Empresa;

public sealed class EmpresaSmtpConfiguracionRequest
{
    [Required, StringLength(255, MinimumLength = 3)]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Puerto { get; set; } = 587;

    public bool UsarSsl { get; set; } = true;
    public bool RequiereAutenticacion { get; set; } = true;

    [StringLength(255)]
    public string? Usuario { get; set; }

    // Vacío al consultar o guardar sin cambiar la clave. Nunca se devuelve desde la API.
    [StringLength(500)]
    public string? Contrasena { get; set; }

    [Required, EmailAddress, StringLength(180)]
    public string CorreoRemitente { get; set; } = string.Empty;

    [Required, StringLength(180, MinimumLength = 1)]
    public string NombreRemitente { get; set; } = "EasyMarket Systems";

    public bool Habilitado { get; set; }
}

public sealed class EmpresaSmtpPruebaRequest
{
    [Required, EmailAddress, StringLength(180)]
    public string Destinatario { get; set; } = string.Empty;
}
