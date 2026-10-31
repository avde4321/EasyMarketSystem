using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Contabilidad;

public sealed class CerrarPeriodoFiscalRequest
{
    [Range(2000, 2100, ErrorMessage = "El año fiscal no es válido.")]
    public int Anio { get; set; }

    [Range(1, 12, ErrorMessage = "El mes fiscal debe estar entre 1 y 12.")]
    public int Mes { get; set; }
}
