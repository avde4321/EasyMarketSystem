using TestDeIa.Domain.Modules.Tesoreria.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ExtractoBancarioDetalleEntity : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid CuentaBancariaId { get; set; }
    public Guid ExtractoHeaderId { get; set; }
    public DateTime FechaTransaccion { get; set; }
    public string NumeroDocumentoRef { get; set; } = string.Empty;
    public string ConceptoDescripcion { get; set; } = string.Empty;
    public TipoMovimientoBancario TipoMovimiento { get; set; }
    public decimal Monto { get; set; }
    public bool Conciliado { get; set; }
    public Guid? MovimientoTesoreriaId { get; set; }

    public ExtractoBancarioHeaderEntity ExtractoHeader { get; set; } = default!;
    public MovimientoTesoreriaEntity? MovimientoTesoreria { get; set; }
}
