namespace TestDeIa.Domain.Modules.Caja.Entities;

public sealed class CajaSesion
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTimeOffset FechaApertura { get; set; }
    public DateTimeOffset? FechaCierre { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal MontoInicial
    {
        get => MontoApertura;
        set => MontoApertura = value;
    }

    public decimal TotalVentasEfectivoCalculado { get; set; }
    public decimal TotalVentasTarjetaCalculado { get; set; }
    public decimal TotalVentasTransferenciaCalculado { get; set; }
    public decimal MontoFisicoEfectivoReal { get; set; }
    public decimal MontoFisicoTarjetaReal { get; set; }
    public decimal MontoFisicoTransferenciaReal { get; set; }
    public decimal EfectivoReportado
    {
        get => MontoFisicoEfectivoReal;
        set => MontoFisicoEfectivoReal = value;
    }

    public decimal TarjetaReportada
    {
        get => MontoFisicoTarjetaReal;
        set => MontoFisicoTarjetaReal = value;
    }

    public decimal TransferenciaReportada
    {
        get => MontoFisicoTransferenciaReal;
        set => MontoFisicoTransferenciaReal = value;
    }

    public decimal DiferenciaEfectivo { get; set; }
    public decimal DiferenciaTarjeta { get; set; }
    public decimal DiferenciaTransferencia { get; set; }
    public decimal Diferencia { get; set; }
    public CajaEstado EstadoCaja { get; set; }
    public CajaEstado Estado
    {
        get => EstadoCaja;
        set => EstadoCaja = value;
    }

    public Guid? AsientoContableId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UsuarioCreacionId { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UsuarioModificacionId { get; set; }
}
