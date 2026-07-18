namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class AsientoDetalleEntity
{
    public Guid Id { get; set; }
    public Guid AsientoContableId { get; set; }
    public Guid CuentaContableId { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }

    public AsientoContableEntity AsientoContable { get; set; } = default!;
    public CuentaContableEntity CuentaContable { get; set; } = default!;
}
