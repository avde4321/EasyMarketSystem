namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SaasPlanEntity
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int LimiteFacturasMensuales { get; set; }

    public int LimiteUsuarios { get; set; }

    public int LimiteSucursales { get; set; }

    public bool PermiteModuloSRI { get; set; }

    public bool PermiteModuloKardexAvanzado { get; set; }

    public decimal PrecioMensual { get; set; }

    public bool EsActivo { get; set; } = true;

    public ICollection<SaasSuscripcionEntity> Suscripciones { get; set; } = [];
}
