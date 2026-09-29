namespace TestDeIa.Domain.Modules.Caja.Entities;

public static class TipoMovimientoCaja
{
    public const string IngresoManual = "INGRESO_MANUAL";
    public const string EgresoGasto = "EGRESO_GASTO";
    public const string RetiroSeguridad = "RETIRO_SEGURIDAD";

    public static readonly string[] All =
    [
        IngresoManual,
        EgresoGasto,
        RetiroSeguridad
    ];

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && All.Contains(value.Trim().ToUpperInvariant());
}
