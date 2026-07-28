namespace TestDeIa.Shared.Responses.Contabilidad;

public sealed class PeriodoContableResponse
{
    public Guid Id { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public bool EstaCerrado { get; set; }
    public DateTimeOffset? FechaCierre { get; set; }
    public Guid? UsuarioCierreId { get; set; }
}
