using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Contabilidad;

public sealed class CrearAsientoRequest
{
    [Required]
    public DateTime FechaContable { get; set; } = DateTime.Today;

    [Required]
    [StringLength(300)]
    public string Concepto { get; set; } = string.Empty;

    [Required]
    public string ModuloOrigen { get; set; } = "Diario";

    [StringLength(49)]
    public string? DocumentoSoporte { get; set; }

    [Required]
    public string Estado { get; set; } = "Posteado";

    [MinLength(1)]
    public List<CrearAsientoDetalleRequest> Detalles { get; set; } = [];
}
