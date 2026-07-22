namespace TestDeIa.Shared.Responses.Caja;

public sealed class CajaSesionResponse
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
    public decimal EfectivoReportado { get; set; }
    public decimal TarjetaReportada { get; set; }
    public decimal TransferenciaReportada { get; set; }
    public decimal DiferenciaEfectivo { get; set; }
    public decimal DiferenciaTarjeta { get; set; }
    public decimal DiferenciaTransferencia { get; set; }
    public decimal Diferencia { get; set; }
    public Guid? AsientoContableId { get; set; }
    public string EstadoCaja { get; set; } = string.Empty;
}
