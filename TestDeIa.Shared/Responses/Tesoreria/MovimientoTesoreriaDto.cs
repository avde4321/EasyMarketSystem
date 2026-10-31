namespace TestDeIa.Shared.Responses.Tesoreria;

public sealed class MovimientoTesoreriaDto
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid CuentaBancariaId { get; set; }
    public string BancoNombre { get; set; } = string.Empty;
    public string NumeroCuenta { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public byte Tipo { get; set; }
    public string TipoNombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Beneficiario { get; set; } = string.Empty;
    public Guid? FacturaVentaId { get; set; }
    public Guid? CompraId { get; set; }
    public Guid? AsientoContableId { get; set; }
    public byte EstadoConciliacion { get; set; }
    public string EstadoConciliacionNombre { get; set; } = string.Empty;
}
