namespace TestDeIa.Shared.Responses.Saas;

public sealed record SaasPagoResponse(Guid Id, Guid EmpresaId, int SaasPlanId, decimal Monto,
    DateTime FechaPago, string MetodoPago, string EstadoPago, string ComprobanteReferencia,
    DateTime? FechaConfirmacion, DateOnly? FechaPagoInformada = null, int PeriodoMeses = 1);

public sealed record SaasBillingPortalResponse(bool SuscripcionActiva, string? EstadoSuscripcion,
    DateTime? FechaVencimiento, IReadOnlyCollection<SaasPagoResponse> PagosPendientes, SaasPagoResponse? UltimoPago,
    int? SaasPlanId = null, string? PlanNombre = null);

public sealed record SaasCheckoutResponse(bool Disponible, string Mensaje, string? UrlPago = null);
