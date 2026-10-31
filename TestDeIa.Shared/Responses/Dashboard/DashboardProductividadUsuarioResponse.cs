namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardProductividadUsuarioResponse
{
    public Guid UsuarioId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public int Comprobantes { get; set; }
    public decimal TotalVentas { get; set; }
}
