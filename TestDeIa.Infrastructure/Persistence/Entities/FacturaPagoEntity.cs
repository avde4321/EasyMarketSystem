namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class FacturaPagoEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid FacturaId { get; set; }
    public Guid CajaSesionId { get; set; }
    public string FormaPagoCodigo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? LoteNumero { get; set; }
    public string? VoucherNumero { get; set; }
    public string? BancoNombre { get; set; }
    public string? NumeroReferencia { get; set; }

    public FacturaEntity Factura { get; set; } = default!;
    public CajaSesionEntity CajaSesion { get; set; } = default!;
}
