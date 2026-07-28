using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class TransferenciaInventarioFormalRequest
{
    [Required]
    public Guid BodegaOrigenId { get; set; }

    [Required]
    public Guid BodegaDestinoId { get; set; }

    [StringLength(250)]
    public string MotivoTraslado { get; set; } = string.Empty;

    public Guid? GuiaRemisionId { get; set; }

    public List<TransferenciaInventarioDetalleRequest> Detalles { get; set; } = [];
}
