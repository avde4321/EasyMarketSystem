namespace TestDeIa.Domain.Modules.Facturacion.Entities;

public sealed class Factura
{
    public Factura(
        Guid id,
        Guid empresaId,
        long secuencial,
        string establecimiento,
        string puntoEmision,
        string rucEmisor,
        string razonSocialEmisor,
        string? nombreComercialEmisor,
        string direccionMatrizEmisor,
        string? direccionEstablecimientoEmisor,
        string ambienteSri,
        string tipoEmision,
        bool obligadoContabilidad,
        string? contribuyenteEspecial,
        string? regimenRimpe,
        string? agenteRetencionResolucion,
        Guid clienteId,
        string clienteTipoIdentificacion,
        string clienteIdentificacion,
        string clienteNombre,
        string? clienteDireccion,
        string? clienteEmail,
        string? clienteTelefono,
        string formaPago,
        string formaPagoSriCodigo,
        FacturaEstado estado,
        decimal subtotal,
        decimal totalDescuento,
        decimal subtotalIva0,
        decimal subtotalIva5,
        decimal subtotalIva8,
        decimal subtotalIva15,
        decimal ivaTotal,
        decimal total,
        string? observacion,
        string? claveAcceso,
        string? numeroAutorizacion,
        string? mensajeEstado,
        string? xmlGenerado,
        string? xmlFirmado,
        DateTimeOffset fechaEmision,
        DateTimeOffset? fechaAutorizacion,
        IReadOnlyCollection<FacturaDetalle> detalles)
    {
        Id = id;
        EmpresaId = empresaId;
        Secuencial = secuencial;
        Establecimiento = establecimiento;
        PuntoEmision = puntoEmision;
        RucEmisor = rucEmisor;
        RazonSocialEmisor = razonSocialEmisor;
        NombreComercialEmisor = nombreComercialEmisor;
        DireccionMatrizEmisor = direccionMatrizEmisor;
        DireccionEstablecimientoEmisor = direccionEstablecimientoEmisor;
        AmbienteSri = ambienteSri;
        TipoEmision = tipoEmision;
        ObligadoContabilidad = obligadoContabilidad;
        ContribuyenteEspecial = contribuyenteEspecial;
        RegimenRimpe = regimenRimpe;
        AgenteRetencionResolucion = agenteRetencionResolucion;
        ClienteId = clienteId;
        ClienteTipoIdentificacion = clienteTipoIdentificacion;
        ClienteIdentificacion = clienteIdentificacion;
        ClienteNombre = clienteNombre;
        ClienteDireccion = clienteDireccion;
        ClienteEmail = clienteEmail;
        ClienteTelefono = clienteTelefono;
        FormaPago = formaPago;
        FormaPagoSriCodigo = formaPagoSriCodigo;
        Estado = estado;
        Subtotal = subtotal;
        TotalDescuento = totalDescuento;
        SubtotalIva0 = subtotalIva0;
        SubtotalIva5 = subtotalIva5;
        SubtotalIva8 = subtotalIva8;
        SubtotalIva15 = subtotalIva15;
        IvaTotal = ivaTotal;
        Total = total;
        Observacion = observacion;
        ClaveAcceso = claveAcceso;
        NumeroAutorizacion = numeroAutorizacion;
        MensajeEstado = mensajeEstado;
        XmlGenerado = xmlGenerado;
        XmlFirmado = xmlFirmado;
        FechaEmision = fechaEmision;
        FechaAutorizacion = fechaAutorizacion;
        Detalles = detalles;
    }

    public Guid Id { get; }

    public Guid EmpresaId { get; }

    public long Secuencial { get; }

    public string Establecimiento { get; }

    public string PuntoEmision { get; }

    public string RucEmisor { get; }

    public string RazonSocialEmisor { get; }

    public string? NombreComercialEmisor { get; }

    public string DireccionMatrizEmisor { get; }

    public string? DireccionEstablecimientoEmisor { get; }

    public string AmbienteSri { get; }

    public string TipoEmision { get; }

    public bool ObligadoContabilidad { get; }

    public string? ContribuyenteEspecial { get; }

    public string? RegimenRimpe { get; }

    public string? AgenteRetencionResolucion { get; }

    public Guid ClienteId { get; }

    public string ClienteTipoIdentificacion { get; }

    public string ClienteIdentificacion { get; }

    public string ClienteNombre { get; }

    public string? ClienteDireccion { get; }

    public string? ClienteEmail { get; }

    public string? ClienteTelefono { get; }

    public string FormaPago { get; }

    public string FormaPagoSriCodigo { get; }

    public FacturaEstado Estado { get; }

    public decimal Subtotal { get; }

    public decimal TotalDescuento { get; }

    public decimal SubtotalIva0 { get; }

    public decimal SubtotalIva5 { get; }

    public decimal SubtotalIva8 { get; }

    public decimal SubtotalIva15 { get; }

    public decimal IvaTotal { get; }

    public decimal Total { get; }

    public string? Observacion { get; }

    public string? ClaveAcceso { get; }

    public string? NumeroAutorizacion { get; }

    public string? MensajeEstado { get; }

    public string? XmlGenerado { get; }

    public string? XmlFirmado { get; }

    public DateTimeOffset FechaEmision { get; }

    public DateTimeOffset? FechaAutorizacion { get; }

    public IReadOnlyCollection<FacturaDetalle> Detalles { get; }
}
