using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Saas;

public sealed class SaasPagosAdminQuery : IValidatableObject
{
    [StringLength(150)] public string? Term { get; set; }
    public string? Estado { get; set; } = "Pendiente";
    public Guid? EmpresaId { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    [Range(0, int.MaxValue)] public int Skip { get; set; }
    [Range(1, 50)] public int Take { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Estado) && Estado.Trim().ToUpperInvariant() is not ("TODOS" or "PENDIENTE" or "APROBADO" or "RECHAZADO"))
            yield return new ValidationResult("El estado del pago no es válido.", [nameof(Estado)]);
        if (EmpresaId == Guid.Empty) yield return new ValidationResult("La empresa no es válida.", [nameof(EmpresaId)]);
        if (FechaDesde > FechaHasta) yield return new ValidationResult("La fecha inicial no puede ser posterior a la final.", [nameof(FechaDesde)]);
        if (FechaHasta == DateOnly.MaxValue) yield return new ValidationResult("La fecha final está fuera del rango admitido.", [nameof(FechaHasta)]);
    }
}
