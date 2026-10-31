namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardVentasComparativoResponse
{
    public decimal MesActual { get; set; }
    public decimal MismoMesAnioAnterior { get; set; }
    public decimal PorcentajeVariacion { get; set; }
}
