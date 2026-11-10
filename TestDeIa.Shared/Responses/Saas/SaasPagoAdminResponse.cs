namespace TestDeIa.Shared.Responses.Saas;

public sealed record SaasPagoAdminResponse(
    Guid Id, Guid EmpresaId, string EmpresaNombre, string EmpresaRuc,
    int SaasPlanId, string PlanNombre, bool PlanActivo, decimal Monto,
    DateTime FechaRegistro, DateOnly? FechaPagoInformada, string MetodoPago, string EstadoPago,
    string ComprobanteReferencia, string? Observaciones,
    DateTime? FechaConfirmacion, string? ObservacionesConfirmacion, Guid? ConfirmadoPorUsuarioId,
    int PeriodoMeses = 1);
