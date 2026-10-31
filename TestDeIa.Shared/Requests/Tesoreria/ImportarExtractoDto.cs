using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Tesoreria;

public sealed class ImportarExtractoDto
{
    [Required]
    public Guid CuentaBancariaId { get; set; }

    [Required]
    public byte[] Archivo { get; set; } = [];

    [Required]
    [MaxLength(255)]
    public string NombreArchivoOriginal { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Delimitador { get; set; }

    [MaxLength(30)]
    public string? Formato { get; set; }

    public DateTime FechaDesde { get; set; }

    public DateTime FechaHasta { get; set; }
}
