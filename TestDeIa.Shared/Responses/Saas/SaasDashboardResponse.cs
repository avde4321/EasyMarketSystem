namespace TestDeIa.Shared.Responses.Saas;

public sealed record SaasDashboardResponse(int SuscripcionesActivas, int DemosVigentes,
    int SuscripcionesVencidas, int PagosPendientes, decimal CobrosAprobadosMes, DateTime DesdeUtc, DateTime HastaUtc);
