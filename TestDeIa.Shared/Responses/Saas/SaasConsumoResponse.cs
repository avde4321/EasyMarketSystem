namespace TestDeIa.Shared.Responses.Saas;

public sealed record SaasConsumoResponse(Guid EmpresaId, string EmpresaNombre, string Ruc,
    string Plan, string EstadoSuscripcion, DateTime Vencimiento, bool PuedeOperar, bool CuotasAplicadas,
    int LimiteMensual, int ConsumoCupo, int Reservadas, int? Disponibles, decimal? PorcentajeUso,
    int GeneradasMes, int AutorizadasMes, int AutorizadasMesAnterior, decimal? CrecimientoPorcentaje,
    int ProyeccionAutorizadasMes, string Evaluacion, string Motivo, string? PlanSugerido,
    DateTime PeriodoUtc, DateTime ConsultadoUtc, bool ContadorDifiere, int SuscripcionesRegistradas);
