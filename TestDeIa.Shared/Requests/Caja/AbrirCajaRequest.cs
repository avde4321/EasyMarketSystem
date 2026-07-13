using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Caja;

public sealed class AbrirCajaRequest
{
    [Range(0, 999999999, ErrorMessage = "El monto de apertura debe ser positivo.")]
    public decimal MontoApertura { get; set; }
}
