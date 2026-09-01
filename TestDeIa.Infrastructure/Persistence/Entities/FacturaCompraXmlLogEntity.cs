using TestDeIa.Domain.Modules.XmlSri.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class FacturaCompraXmlLogEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string ClaveAcceso { get; set; } = string.Empty;

    public string RucEmisor { get; set; } = string.Empty;

    public string RazonSocialEmisor { get; set; } = string.Empty;

    public string RucComprador { get; set; } = string.Empty;

    public DateTimeOffset FechaEmision { get; set; }

    public string CodDoc { get; set; } = string.Empty;

    public string EstabPuntoEmiSecuencial { get; set; } = string.Empty;

    public decimal TotalSinImpuestos { get; set; }

    public decimal TotalDescuento { get; set; }

    public decimal ImporteTotal { get; set; }

    public string XmlContenido { get; set; } = string.Empty;

    public EstadoProcesamientoXmlSri EstadoProcesamiento { get; set; } = EstadoProcesamientoXmlSri.Pendiente;

    public Guid? CompraId { get; set; }

    public CompraEntity? Compra { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
