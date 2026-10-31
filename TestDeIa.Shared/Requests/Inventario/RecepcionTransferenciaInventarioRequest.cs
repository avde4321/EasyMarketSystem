namespace TestDeIa.Shared.Requests.Inventario;

public sealed class RecepcionTransferenciaInventarioRequest
{
    public List<RecepcionTransferenciaInventarioDetalleRequest> Detalles { get; set; } = [];
}
