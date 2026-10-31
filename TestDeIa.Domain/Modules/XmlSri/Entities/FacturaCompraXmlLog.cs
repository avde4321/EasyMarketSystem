using TestDeIa.Domain.Modules.XmlSri.Enums;

namespace TestDeIa.Domain.Modules.XmlSri.Entities;

public sealed record FacturaCompraXmlLog(
    Guid Id,
    Guid EmpresaId,
    string ClaveAcceso,
    string RucEmisor,
    string RazonSocialEmisor,
    string RucComprador,
    DateTime FechaEmision,
    string CodDoc,
    string EstabPuntoEmiSecuencial,
    decimal TotalSinImpuestos,
    decimal TotalDescuento,
    decimal ImporteTotal,
    string XmlContenido,
    EstadoProcesamientoXmlSri EstadoProcesamiento,
    Guid? CompraId);
