namespace TestDeIa.Shared.Responses.XmlSri;

public sealed class FacturaCompraXmlLogResponse
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string ClaveAcceso { get; set; } = string.Empty;

    public string RucEmisor { get; set; } = string.Empty;

    public string RazonSocialEmisor { get; set; } = string.Empty;

    public string RucComprador { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    public string CodDoc { get; set; } = string.Empty;

    public string EstabPuntoEmiSecuencial { get; set; } = string.Empty;

    public decimal TotalSinImpuestos { get; set; }

    public decimal TotalDescuento { get; set; }

    public decimal ImporteTotal { get; set; }

    public int EstadoProcesamiento { get; set; }

    public string EstadoProcesamientoNombre { get; set; } = string.Empty;

    public Guid? CompraId { get; set; }
}
