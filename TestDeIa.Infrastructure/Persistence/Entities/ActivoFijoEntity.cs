using TestDeIa.Domain.Modules.ActivosFijos.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ActivoFijoEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid? CompraDetalleId { get; set; }
    public CompraDetalleEntity? CompraDetalle { get; set; }
    public string CodigoActivo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? SerieMarca { get; set; }
    public CategoriaSriActivoFijo CategoriaSRI { get; set; } = CategoriaSriActivoFijo.EquiposComputo;
    public DateTime FechaAdquisicion { get; set; }
    public decimal CostoInicial { get; set; }
    public decimal ValorResidual { get; set; }
    public int VidaUtilAnios { get; set; }
    public decimal PorcentajeDepreciacionAnual { get; set; }
    public string? UbicacionFisica { get; set; }
    public string? CustodioResponsable { get; set; }
    public EstadoActivoFijo EstadoActivo { get; set; } = EstadoActivoFijo.Activo;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
