namespace TestDeIa.Domain.Modules.Contabilidad.Entities;

public sealed class AsientoDetalle
{
    public AsientoDetalle(Guid id, Guid cuentaContableId, decimal debe, decimal haber)
    {
        Id = id;
        CuentaContableId = cuentaContableId;
        Debe = debe;
        Haber = haber;
    }

    public Guid Id { get; }
    public Guid CuentaContableId { get; }
    public decimal Debe { get; }
    public decimal Haber { get; }
}
