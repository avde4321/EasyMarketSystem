using TestDeIa.Domain.Modules.Tesoreria.Enums;

namespace TestDeIa.Domain.Modules.Tesoreria.Entities;

public sealed class MovimientoTesoreria
{
    public Guid Id { get; init; }
    public Guid EmpresaId { get; init; }
    public Guid CuentaBancariaId { get; init; }
    public DateTime Fecha { get; init; }
    public TipoMovimientoTesoreria Tipo { get; init; }
    public decimal Monto { get; init; }
    public string Beneficiario { get; init; } = string.Empty;
    public Guid? FacturaVentaId { get; init; }
    public Guid? CompraId { get; init; }
    public Guid? AsientoContableId { get; init; }
    public EstadoConciliacionTesoreria EstadoConciliacion { get; init; }
}
