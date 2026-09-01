namespace TestDeIa.Shared.Responses.XmlSri;

public sealed class XmlSriParserResultDto
{
    public string ClaveAcceso { get; set; } = string.Empty;

    public string Ambiente { get; set; } = string.Empty;

    public string TipoEmision { get; set; } = string.Empty;

    public string RazonSocialEmisor { get; set; } = string.Empty;

    public string NombreComercialEmisor { get; set; } = string.Empty;

    public string RucEmisor { get; set; } = string.Empty;

    public string CodDoc { get; set; } = string.Empty;

    public string Establecimiento { get; set; } = string.Empty;

    public string PuntoEmision { get; set; } = string.Empty;

    public string Secuencial { get; set; } = string.Empty;

    public string EstabPuntoEmiSecuencial => $"{Establecimiento}-{PuntoEmision}-{Secuencial}";

    public string DireccionMatriz { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    public string DireccionEstablecimiento { get; set; } = string.Empty;

    public string TipoIdentificacionComprador { get; set; } = string.Empty;

    public string RazonSocialComprador { get; set; } = string.Empty;

    public string RucComprador { get; set; } = string.Empty;

    public decimal TotalSinImpuestos { get; set; }

    public decimal TotalDescuento { get; set; }

    public decimal Propina { get; set; }

    public decimal ImporteTotal { get; set; }

    public string Moneda { get; set; } = "DOLAR";

    public IReadOnlyCollection<XmlSriImpuestoTotalDto> TotalConImpuestos { get; set; } = Array.Empty<XmlSriImpuestoTotalDto>();

    public IReadOnlyCollection<XmlSriDetalleFacturaDto> Detalles { get; set; } = Array.Empty<XmlSriDetalleFacturaDto>();

    public IReadOnlyDictionary<string, string> InformacionAdicional { get; set; } = new Dictionary<string, string>();

    public string XmlContenido { get; set; } = string.Empty;
}

public sealed class XmlSriImpuestoTotalDto
{
    public string Codigo { get; set; } = string.Empty;

    public string CodigoPorcentaje { get; set; } = string.Empty;

    public decimal BaseImponible { get; set; }

    public decimal Valor { get; set; }
}

public sealed class XmlSriDetalleFacturaDto
{
    public string CodigoPrincipal { get; set; } = string.Empty;

    public string? CodigoAuxiliar { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal PrecioTotalSinImpuesto { get; set; }

    public IReadOnlyCollection<XmlSriDetalleImpuestoDto> Impuestos { get; set; } = Array.Empty<XmlSriDetalleImpuestoDto>();
}

public sealed class XmlSriDetalleImpuestoDto
{
    public string Codigo { get; set; } = string.Empty;

    public string CodigoPorcentaje { get; set; } = string.Empty;

    public decimal Tarifa { get; set; }

    public decimal BaseImponible { get; set; }

    public decimal Valor { get; set; }
}
