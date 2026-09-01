using TestDeIa.Shared.Compras;

namespace TestDeIa.Shared.Requests.XmlSri;

public sealed class ConvertirXmlACompraRequest
{
    public Guid FacturaCompraXmlLogId { get; set; }

    public Guid BodegaId { get; set; }

    public Guid? ProveedorId { get; set; }

    public bool CrearProveedorSiNoExiste { get; set; } = true;

    public FormaPagoCompra FormaPagoCompra { get; set; } = FormaPagoCompra.CreditoProveedores;

    public int DiasCredito { get; set; } = 30;

    public IReadOnlyCollection<MapeoProductoProveedorDto> MapeosProductos { get; set; } = Array.Empty<MapeoProductoProveedorDto>();
}
