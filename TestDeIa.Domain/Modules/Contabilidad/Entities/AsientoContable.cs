using TestDeIa.Domain.Modules.Contabilidad.Enums;

namespace TestDeIa.Domain.Modules.Contabilidad.Entities;

public sealed class AsientoContable
{
    public AsientoContable(
        Guid id,
        Guid empresaId,
        string numeroAsiento,
        DateTime fechaContable,
        string concepto,
        ModuloOrigenContable moduloOrigen,
        string? documentoSoporte,
        EstadoAsientoContable estado,
        IReadOnlyCollection<AsientoDetalle> detalles)
    {
        Id = id;
        EmpresaId = empresaId;
        NumeroAsiento = numeroAsiento;
        FechaContable = fechaContable;
        Concepto = concepto;
        ModuloOrigen = moduloOrigen;
        DocumentoSoporte = documentoSoporte;
        Estado = estado;
        Detalles = detalles;
    }

    public Guid Id { get; }
    public Guid EmpresaId { get; }
    public string NumeroAsiento { get; }
    public DateTime FechaContable { get; }
    public string Concepto { get; }
    public ModuloOrigenContable ModuloOrigen { get; }
    public string? DocumentoSoporte { get; }
    public EstadoAsientoContable Estado { get; }
    public IReadOnlyCollection<AsientoDetalle> Detalles { get; }
}
