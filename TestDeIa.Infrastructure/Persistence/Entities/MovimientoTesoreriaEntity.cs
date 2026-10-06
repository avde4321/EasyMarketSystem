using TestDeIa.Domain.Modules.Tesoreria.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class MovimientoTesoreriaEntity : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid CuentaBancariaId { get; set; }
    public DateTime Fecha { get; set; }
    public TipoMovimientoTesoreria Tipo { get; set; }
    public decimal Monto { get; set; }
    public string Beneficiario { get; set; } = string.Empty;
    public Guid? FacturaVentaId { get; set; }
    public Guid? CompraId { get; set; }
    public Guid? AsientoContableId { get; set; }
    public EstadoConciliacionTesoreria EstadoConciliacion { get; set; }

    public EmpresaEmisoraEntity Empresa { get; set; } = default!;
    public CuentaBancariaEntity CuentaBancaria { get; set; } = default!;
    public FacturaEntity? FacturaVenta { get; set; }
    public CompraEntity? Compra { get; set; }
    public AsientoContableEntity? AsientoContable { get; set; }
    public ICollection<ExtractoBancarioDetalleEntity> ExtractosConciliados { get; set; } = [];
}
