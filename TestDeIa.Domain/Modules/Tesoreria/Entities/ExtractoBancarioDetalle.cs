using TestDeIa.Domain.Modules.Tesoreria.Enums;

namespace TestDeIa.Domain.Modules.Tesoreria.Entities;

public sealed class ExtractoBancarioDetalle
{
    public Guid Id { get; init; }
    public Guid ExtractoHeaderId { get; init; }
    public DateTime FechaTransaccion { get; init; }
    public string NumeroDocumentoRef { get; init; } = string.Empty;
    public string ConceptoDescripcion { get; init; } = string.Empty;
    public TipoMovimientoBancario TipoMovimiento { get; init; }
    public decimal Monto { get; init; }
    public bool Conciliado { get; init; }
    public Guid? MovimientoTesoreriaId { get; init; }
}
