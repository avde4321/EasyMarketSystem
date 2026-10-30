namespace TestDeIa.Shared.Responses.Saas;

public sealed record SaasPagoResponse(Guid Id, Guid EmpresaId, int SaasPlanId, decimal Monto,
    DateTime FechaPago, string MetodoPago, string EstadoPago, string ComprobanteReferencia,
    DateTime? FechaConfirmacion, DateOnly? FechaPagoInformada = null);

public sealed record SaasBillingPortalResponse(bool SuscripcionActiva, string? EstadoSuscripcion,
    DateTime? FechaVencimiento, IReadOnlyCollection<SaasPagoResponse> PagosPendientes, SaasPagoResponse? UltimoPago);

public sealed record SaasCheckoutResponse(bool Disponible, string Mensaje, string? UrlPago = null);
