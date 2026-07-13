namespace TestDeIa.Shared.Responses.Caja;

public sealed class CajaSesionResponse
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTimeOffset FechaApertura { get; set; }
    public DateTimeOffset? FechaCierre { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal TotalVentasEfectivoCalculado { get; set; }
    public decimal TotalVentasTarjetaCalculado { get; set; }
    public decimal MontoFisicoEfectivoReal { get; set; }
    public decimal MontoFisicoTarjetaReal { get; set; }
    public decimal DiferenciaEfectivo { get; set; }
    public decimal DiferenciaTarjeta { get; set; }
    public string EstadoCaja { get; set; } = string.Empty;
}
