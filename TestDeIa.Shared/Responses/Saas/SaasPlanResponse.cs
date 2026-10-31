namespace TestDeIa.Shared.Responses.Saas;

public sealed record SaasPlanResponse(
    int Id,
    string Nombre,
    int LimiteFacturasMensuales,
    int LimiteUsuarios,
    int LimiteSucursales,
    bool PermiteModuloSRI,
    bool PermiteModuloKardexAvanzado,
    decimal PrecioMensual,
    bool EsActivo);
