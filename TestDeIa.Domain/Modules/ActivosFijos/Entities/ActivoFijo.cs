using TestDeIa.Domain.Modules.ActivosFijos.Enums;

namespace TestDeIa.Domain.Modules.ActivosFijos.Entities;

public sealed class ActivoFijo
{
    public ActivoFijo(
        Guid id,
        Guid empresaId,
        Guid? compraDetalleId,
        string codigoActivo,
        string nombre,
        string? serieMarca,
        CategoriaSriActivoFijo categoriaSRI,
        DateTime fechaAdquisicion,
        decimal costoInicial,
        decimal valorResidual,
        int vidaUtilAnios,
        decimal porcentajeDepreciacionAnual,
        string? ubicacionFisica,
        string? custodioResponsable,
        EstadoActivoFijo estadoActivo,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        EmpresaId = empresaId;
        CompraDetalleId = compraDetalleId;
        CodigoActivo = codigoActivo;
        Nombre = nombre;
        SerieMarca = serieMarca;
        CategoriaSRI = categoriaSRI;
        FechaAdquisicion = fechaAdquisicion;
        CostoInicial = costoInicial;
        ValorResidual = valorResidual;
        VidaUtilAnios = vidaUtilAnios;
        PorcentajeDepreciacionAnual = porcentajeDepreciacionAnual;
        UbicacionFisica = ubicacionFisica;
        CustodioResponsable = custodioResponsable;
        EstadoActivo = estadoActivo;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }
    public Guid EmpresaId { get; }
    public Guid? CompraDetalleId { get; }
    public string CodigoActivo { get; }
    public string Nombre { get; }
    public string? SerieMarca { get; }
    public CategoriaSriActivoFijo CategoriaSRI { get; }
    public DateTime FechaAdquisicion { get; }
    public decimal CostoInicial { get; }
    public decimal ValorResidual { get; }
    public int VidaUtilAnios { get; }
    public decimal PorcentajeDepreciacionAnual { get; }
    public string? UbicacionFisica { get; }
    public string? CustodioResponsable { get; }
    public EstadoActivoFijo EstadoActivo { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; }
}
