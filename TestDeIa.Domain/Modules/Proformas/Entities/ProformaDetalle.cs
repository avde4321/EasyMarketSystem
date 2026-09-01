namespace TestDeIa.Domain.Modules.Proformas.Entities;

public sealed record ProformaDetalle(
    Guid Id,
    Guid ProformaId,
    Guid ProductoId,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal Descuento,
    decimal TarifaIVA,
    decimal ValorIVA,
    decimal Subtotal);
