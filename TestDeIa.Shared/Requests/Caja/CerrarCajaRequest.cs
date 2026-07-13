using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Caja;

public sealed class CerrarCajaRequest
{
    [Range(0, 999999999, ErrorMessage = "El monto fisico en efectivo debe ser positivo.")]
    public decimal MontoFisicoEfectivoReal { get; set; }

    [Range(0, 999999999, ErrorMessage = "El monto fisico en tarjeta debe ser positivo.")]
    public decimal MontoFisicoTarjetaReal { get; set; }
}
