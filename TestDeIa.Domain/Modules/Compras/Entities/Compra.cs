using TestDeIa.Domain.Modules.Compras.Enums;

namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class Compra
{
    public Compra(
        Guid id,
        Guid empresaId,
        Guid proveedorId,
        Guid bodegaId,
        NaturalezaCompra naturalezaCompra,
        string tipoDocumentoCodigo,
        string tipoComprobanteSri,
        string sustentoTributarioSri,
        string establecimiento,
        string puntoEmision,
        string secuencial,
        string? claveAccesoProveedor,
        string? claveAccesoGenerada,
        string? numeroAutorizacion,
        TestDeIa.Domain.Modules.Facturacion.Entities.FacturaEstado? estadoSri,
        string? mensajeEstado,
        string formaPagoSriCodigo,
        string? observacion,
        string? xmlGenerado,
        string? xmlFirmado,
        string? processingNode,
        DateTimeOffset? processingStartedAt,
        int retryCount,
        DateTimeOffset? nextRetryAt,
        DateTimeOffset fechaEmision,
        decimal subtotalIva0,
        decimal subtotalIva5,
        decimal subtotalIva8,
        decimal subtotalIva15,
        decimal totalDescuento,
        decimal totalImpuestos,
        decimal importeTotal,
        EstadoCompra estadoCompra,
        DateTimeOffset createdAt,
        Guid usuarioCreacionId,
        DateTimeOffset? updatedAt,
        Guid? usuarioModificacionId,
        IReadOnlyCollection<CompraDetalle> detalles)
    {
        Id = id;
        EmpresaId = empresaId;
        ProveedorId = proveedorId;
        BodegaId = bodegaId;
        NaturalezaCompra = naturalezaCompra;
        TipoDocumentoCodigo = tipoDocumentoCodigo;
        TipoComprobanteSRI = tipoComprobanteSri;
        SustentoTributarioSRI = sustentoTributarioSri;
        Establecimiento = establecimiento;
        PuntoEmision = puntoEmision;
        Secuencial = secuencial;
        ClaveAccesoProveedor = claveAccesoProveedor;
        ClaveAccesoGenerada = claveAccesoGenerada;
        NumeroAutorizacion = numeroAutorizacion;
        EstadoSri = estadoSri;
        MensajeEstado = mensajeEstado;
        FormaPagoSriCodigo = formaPagoSriCodigo;
        Observacion = observacion;
        XmlGenerado = xmlGenerado;
        XmlFirmado = xmlFirmado;
        ProcessingNode = processingNode;
        ProcessingStartedAt = processingStartedAt;
        RetryCount = retryCount;
        NextRetryAt = nextRetryAt;
        FechaEmision = fechaEmision;
        SubtotalIva0 = subtotalIva0;
        SubtotalIva5 = subtotalIva5;
        SubtotalIva8 = subtotalIva8;
        SubtotalIva15 = subtotalIva15;
        TotalDescuento = totalDescuento;
        TotalImpuestos = totalImpuestos;
        ImporteTotal = importeTotal;
        EstadoCompra = estadoCompra;
        CreatedAt = createdAt;
        UsuarioCreacionId = usuarioCreacionId;
        UpdatedAt = updatedAt;
        UsuarioModificacionId = usuarioModificacionId;
        Detalles = detalles;
    }

    public Guid Id { get; }
    public Guid EmpresaId { get; }
    public Guid ProveedorId { get; }
    public Guid BodegaId { get; }
    public NaturalezaCompra NaturalezaCompra { get; }
    public string TipoDocumentoCodigo { get; }
    public string TipoComprobanteSRI { get; }
    public string SustentoTributarioSRI { get; }
    public string Establecimiento { get; }
    public string PuntoEmision { get; }
    public string Secuencial { get; }
    public string? ClaveAccesoProveedor { get; }
    public string? ClaveAccesoGenerada { get; }
    public string? NumeroAutorizacion { get; }
    public TestDeIa.Domain.Modules.Facturacion.Entities.FacturaEstado? EstadoSri { get; }
    public string? MensajeEstado { get; }
    public string FormaPagoSriCodigo { get; }
    public string? Observacion { get; }
    public string? XmlGenerado { get; }
    public string? XmlFirmado { get; }
    public string? ProcessingNode { get; }
    public DateTimeOffset? ProcessingStartedAt { get; }
    public int RetryCount { get; }
    public DateTimeOffset? NextRetryAt { get; }
    public DateTimeOffset FechaEmision { get; }
    public decimal SubtotalIva0 { get; }
    public decimal SubtotalIva5 { get; }
    public decimal SubtotalIva8 { get; }
    public decimal SubtotalIva15 { get; }
    public decimal TotalDescuento { get; }
    public decimal TotalImpuestos { get; }
    public decimal ImporteTotal { get; }
    public EstadoCompra EstadoCompra { get; }
    public DateTimeOffset CreatedAt { get; }
    public Guid UsuarioCreacionId { get; }
    public DateTimeOffset? UpdatedAt { get; }
    public Guid? UsuarioModificacionId { get; }
    public IReadOnlyCollection<CompraDetalle> Detalles { get; }
    public string NumeroComprobante => $"{Establecimiento}-{PuntoEmision}-{Secuencial}";
}

