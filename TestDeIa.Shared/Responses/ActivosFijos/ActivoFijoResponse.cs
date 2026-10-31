namespace TestDeIa.Shared.Responses.ActivosFijos;

public sealed class ActivoFijoResponse
{
    public Guid Id { get; set; }
    public Guid? CompraDetalleId { get; set; }
    public string CodigoActivo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? SerieMarca { get; set; }
    public string CategoriaSRI { get; set; } = string.Empty;
    public DateTime FechaAdquisicion { get; set; }
    public decimal CostoInicial { get; set; }
    public decimal ValorResidual { get; set; }
    public int VidaUtilAnios { get; set; }
    public decimal PorcentajeDepreciacionAnual { get; set; }
    public string? UbicacionFisica { get; set; }
    public string? CustodioResponsable { get; set; }
    public string EstadoActivo { get; set; } = string.Empty;
    public decimal BaseDepreciable => Math.Max(0m, CostoInicial - ValorResidual);
}
