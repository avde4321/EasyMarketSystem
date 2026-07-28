using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Contabilidad;

public sealed class CrearAsientoDetalleRequest
{
    [Required]
    public Guid CuentaContableId { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal Debe { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal Haber { get; set; }
}
