using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Tesoreria;

public sealed class ConciliacionManualDto
{
    [Required]
    public Guid ExtractoDetalleId { get; set; }

    [Required]
    public Guid MovimientoTesoreriaId { get; set; }

    public bool EsAjusteAutomatico { get; set; }
}
