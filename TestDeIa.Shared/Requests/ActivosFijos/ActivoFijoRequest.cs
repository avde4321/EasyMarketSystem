using System.ComponentModel.DataAnnotations;
using TestDeIa.Shared.ActivosFijos;

namespace TestDeIa.Shared.Requests.ActivosFijos;

public sealed class ActivoFijoRequest
{
    [Required]
    [StringLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(120)]
    public string? SerieMarca { get; set; }

    public CategoriaSriActivoFijo CategoriaSRI { get; set; } = CategoriaSriActivoFijo.EquiposComputo;

    public DateTime FechaAdquisicion { get; set; } = DateTime.Today;

    [Range(0.01, 999999999)]
    public decimal CostoInicial { get; set; }

    [Range(0, 999999999)]
    public decimal ValorResidual { get; set; }

    [StringLength(160)]
    public string? UbicacionFisica { get; set; }

    [StringLength(160)]
    public string? CustodioResponsable { get; set; }

    public EstadoActivoFijo EstadoActivo { get; set; } = EstadoActivoFijo.Activo;
}
