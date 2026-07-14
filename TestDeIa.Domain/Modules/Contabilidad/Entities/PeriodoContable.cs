namespace TestDeIa.Domain.Modules.Contabilidad.Entities;

public sealed class PeriodoContable
{
    public PeriodoContable(Guid id, Guid empresaId, int anio, int mes, bool estaCerrado)
    {
        Id = id;
        EmpresaId = empresaId;
        Anio = anio;
        Mes = mes;
        EstaCerrado = estaCerrado;
    }

    public Guid Id { get; }
    public Guid EmpresaId { get; }
    public int Anio { get; }
    public int Mes { get; }
    public bool EstaCerrado { get; }
}