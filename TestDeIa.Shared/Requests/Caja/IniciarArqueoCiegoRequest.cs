using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Caja;

public sealed class IniciarArqueoCiegoRequest
{
    [Range(0, 999999999, ErrorMessage = "El monto declarado en efectivo debe ser positivo.")]
    public decimal MontoDeclaradoEfectivo { get; set; }

    [Range(0, 999999999, ErrorMessage = "El monto declarado en tarjetas debe ser positivo.")]
    public decimal MontoDeclaradoTarjetas { get; set; }

    [Range(0, 999999999, ErrorMessage = "El monto declarado en transferencias debe ser positivo.")]
    public decimal MontoDeclaradoTransferencias { get; set; }

    [Range(0, 999999999, ErrorMessage = "El monto declarado en otros debe ser positivo.")]
    public decimal MontoDeclaradoOtros { get; set; }

    [MaxLength(500)]
    public string? ObservacionesCierre { get; set; }
}
