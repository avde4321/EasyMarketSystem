namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class PeriodoContableEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public bool EstaCerrado { get; set; }
    public DateTimeOffset? FechaCierre { get; set; }
    public Guid? UsuarioCierreId { get; set; }
}
