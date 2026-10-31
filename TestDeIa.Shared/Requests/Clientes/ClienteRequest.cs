using System.ComponentModel.DataAnnotations;
using TestDeIa.Shared.Clientes;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Shared.Requests.Clientes;

public sealed class ClienteRequest : IValidatableObject
{
    private string? correoElectronicoPrincipal;
    private string? correoFacturacionElectronica;
    private string? telefonoCelular;
    [Required(ErrorMessage = "El tipo de identificacion es obligatorio.")]
    [StringLength(2, ErrorMessage = "El tipo de identificacion debe usar el codigo oficial SRI.")]
    public string TipoIdentificacion { get; set; } = "05";

    [Required(ErrorMessage = "La identificacion es obligatoria.")]
    [StringLength(30, ErrorMessage = "La identificacion no puede superar 30 caracteres.")]
    public string Identificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razon social o nombres completos son obligatorios.")]
    [StringLength(180, ErrorMessage = "La razon social o nombres completos no pueden superar 180 caracteres.")]
    public string RazonSocialONombresCompletos { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "El nombre comercial no puede superar 150 caracteres.")]
    public string? NombreComercial { get; set; }

    [Required(ErrorMessage = "La direccion principal es obligatoria.")]
    [StringLength(250, ErrorMessage = "La direccion principal no puede superar 250 caracteres.")]
    public string DireccionPrincipal { get; set; } = string.Empty;

    public string? RegionCodigo { get; set; }

    public string? ProvinciaCodigo { get; set; }

    public string? CiudadCodigo { get; set; }

    public string? SectorCodigo { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
    [StringLength(180, ErrorMessage = "El correo no puede superar 180 caracteres.")]
    public string? CorreoElectronicoPrincipal
    {
        get => correoElectronicoPrincipal;
        set => correoElectronicoPrincipal = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    [Phone(ErrorMessage = "El teléfono de contacto no tiene un formato válido.")]
    [StringLength(40, ErrorMessage = "El teléfono de contacto no puede superar 40 caracteres.")]
    public string? TelefonoCelular
    {
        get => telefonoCelular;
        set => telefonoCelular = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public DateOnly? FechaNacimiento { get; set; }

    [StringLength(30, ErrorMessage = "El genero no puede superar 30 caracteres.")]
    public string? Genero { get; set; }

    [EmailAddress(ErrorMessage = "El correo de facturacion no tiene un formato valido.")]
    [StringLength(180, ErrorMessage = "El correo de facturacion no puede superar 180 caracteres.")]
    public string? CorreoFacturacionElectronica
    {
        get => correoFacturacionElectronica;
        set => correoFacturacionElectronica = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    [Required(ErrorMessage = "El tipo de cliente es obligatorio.")]
    [StringLength(20, ErrorMessage = "El tipo de cliente no puede superar 20 caracteres.")]
    public string TipoCliente { get; set; } = "Natural";

    public bool ObligadoContabilidad { get; set; }

    public bool EsContribuyenteEspecial { get; set; }

    public bool PermiteCredito { get; set; }

    [Range(0, 999999999, ErrorMessage = "El limite de credito no puede ser negativo.")]
    public decimal LimiteCredito { get; set; }

    [Range(0, 3650, ErrorMessage = "Los dias maximos de credito no son validos.")]
    public int DiasCreditoMaximo { get; set; }

    [Required(ErrorMessage = "El estado de credito es obligatorio.")]
    [StringLength(20, ErrorMessage = "El estado de credito no puede superar 20 caracteres.")]
    public string EstadoCredito { get; set; } = EstadoCreditoClienteCodes.AlDia;

    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var tipo = TipoClienteCodes.Normalize(TipoCliente);
        if (tipo is null)
            yield return new ValidationResult("Selecciona persona natural o empresa / persona jurídica.", [nameof(TipoCliente)]);
        if (tipo == TipoClienteCodes.Juridico && SriCatalogCodes.NormalizeTipoIdentificacionCode(TipoIdentificacion) != "04")
            yield return new ValidationResult("Una empresa / persona jurídica debe registrarse con RUC.", [nameof(TipoIdentificacion)]);
        var identificacionTipo = SriCatalogCodes.NormalizeTipoIdentificacionCode(TipoIdentificacion);
        var numero = Identificacion?.Trim() ?? string.Empty;
        if (identificacionTipo is null)
            yield return new ValidationResult("Selecciona un tipo de identificación válido.", [nameof(TipoIdentificacion)]);
        if ((identificacionTipo == "04" && numero.Length != 13) || (identificacionTipo == "05" && numero.Length != 10) ||
            (identificacionTipo is "04" or "05" && numero.Any(c => c is < '0' or > '9')))
            yield return new ValidationResult("La cédula debe tener 10 dígitos y el RUC 13 dígitos, sin letras ni separadores.", [nameof(Identificacion)]);
        if (identificacionTipo == "07" && numero != "9999999999999")
            yield return new ValidationResult("Consumidor final debe usar 9999999999999.", [nameof(Identificacion)]);
        if (identificacionTipo == "06" && numero.Length < 3)
            yield return new ValidationResult("El pasaporte debe tener al menos 3 caracteres.", [nameof(Identificacion)]);
        if (identificacionTipo != "04" && (ObligadoContabilidad || EsContribuyenteEspecial))
            yield return new ValidationResult("Los datos tributarios del contribuyente requieren un RUC.", [nameof(ObligadoContabilidad), nameof(EsContribuyenteEspecial)]);
        if (tipo == TipoClienteCodes.Natural && identificacionTipo != "07" &&
            FechaNacimiento > DateOnly.FromDateTime(SriFechaFiscal.EcuadorContinental(DateTimeOffset.UtcNow).DateTime))
            yield return new ValidationResult("La fecha de nacimiento no puede ser futura.", [nameof(FechaNacimiento)]);
        if (EstadoCreditoClienteCodes.Normalize(EstadoCredito) is null)
            yield return new ValidationResult("Selecciona un estado de crédito válido.", [nameof(EstadoCredito)]);
        if (identificacionTipo == "07" && PermiteCredito)
            yield return new ValidationResult("Consumidor final no puede tener una línea de crédito individual.", [nameof(PermiteCredito)]);
    }
}
