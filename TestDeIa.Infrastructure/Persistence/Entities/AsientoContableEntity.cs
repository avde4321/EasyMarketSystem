using TestDeIa.Domain.Modules.Contabilidad.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class AsientoContableEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string NumeroAsiento { get; set; } = string.Empty;
    public DateTime FechaContable { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public ModuloOrigenContable ModuloOrigen { get; set; }
    public string? DocumentoSoporte { get; set; }
    public EstadoAsientoContable Estado { get; set; }
    public ICollection<AsientoDetalleEntity> Detalles { get; set; } = [];
}
