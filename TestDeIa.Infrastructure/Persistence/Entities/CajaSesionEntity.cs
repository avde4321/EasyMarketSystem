using TestDeIa.Domain.Modules.Caja.Entities;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class CajaSesionEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid? PuntoEmisionId { get; set; }
    public Guid? BodegaId { get; set; }
    public DateTimeOffset FechaApertura { get; set; }
    public DateTimeOffset? FechaCierre { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal TotalVentasEfectivoCalculado { get; set; }
    public decimal TotalVentasTarjetaCalculado { get; set; }
    public decimal TotalVentasTransferenciaCalculado { get; set; }
    public decimal MontoFisicoEfectivoReal { get; set; }
    public decimal MontoFisicoTarjetaReal { get; set; }
    public decimal MontoFisicoTransferenciaReal { get; set; }
    public decimal DiferenciaEfectivo { get; set; }
    public decimal DiferenciaTarjeta { get; set; }
    public decimal DiferenciaTransferencia { get; set; }
    public decimal Diferencia { get; set; }
    public decimal? MontoDeclaradoEfectivo { get; set; }
    public decimal? MontoDeclaradoTarjetas { get; set; }
    public decimal? MontoDeclaradoTransferencias { get; set; }
    public decimal? MontoDeclaradoOtros { get; set; }
    public decimal? MontoDeclaradoTotal { get; set; }
    public decimal MontoCalculadoEfectivo { get; set; }
    public decimal MontoCalculadoTarjetas { get; set; }
    public decimal MontoCalculadoTransferencias { get; set; }
    public decimal MontoCalculadoOtros { get; set; }
    public decimal MontoCalculadoTotal { get; set; }
    public decimal DiferenciaMonto { get; set; }
    public string? ObservacionesCierre { get; set; }
    public CajaEstado EstadoCaja { get; set; }
    public Guid? AsientoContableId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UsuarioCreacionId { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UsuarioModificacionId { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
