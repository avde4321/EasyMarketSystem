namespace TestDeIa.Shared.Responses.Inventario;

public sealed class TransferenciaInventarioDetalleResponse
{
    public Guid Id { get; set; }

    public Guid ProductoId { get; set; }

    public string ProductoCodigo { get; set; } = string.Empty;

    public string ProductoNombre { get; set; } = string.Empty;

    public decimal CantidadEnviada { get; set; }

    public decimal CantidadRecibida { get; set; }

    public decimal CostoUnitario { get; set; }
}
