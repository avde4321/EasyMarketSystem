using TestDeIa.Domain.Modules.ActivosFijos.Enums;

namespace TestDeIa.Application.Modules.ActivosFijos.UseCases;

public sealed class ActivoFijoService
{
    public (int VidaUtilAnios, decimal PorcentajeDepreciacionAnual) GetParametrosSri(CategoriaSriActivoFijo categoria)
    {
        return categoria switch
        {
            CategoriaSriActivoFijo.EquiposComputo => (3, 33.33m),
            CategoriaSriActivoFijo.Vehiculos => (5, 20m),
            CategoriaSriActivoFijo.Edificios => (20, 5m),
            CategoriaSriActivoFijo.MaquinariaEquipo => (10, 10m),
            CategoriaSriActivoFijo.MueblesEnseres => (10, 10m),
            _ => (10, 10m)
        };
    }

    public CategoriaSriActivoFijo ResolveCategoriaFromSriCode(string? categoriaSri)
    {
        var normalized = categoriaSri?.Trim() ?? string.Empty;
        if (normalized.Contains("veh", StringComparison.OrdinalIgnoreCase) || normalized.EndsWith(".02", StringComparison.OrdinalIgnoreCase))
        {
            return CategoriaSriActivoFijo.Vehiculos;
        }

        if (normalized.Contains("edif", StringComparison.OrdinalIgnoreCase) || normalized.Contains("inmueble", StringComparison.OrdinalIgnoreCase))
        {
            return CategoriaSriActivoFijo.Edificios;
        }

        if (normalized.Contains("maquinaria", StringComparison.OrdinalIgnoreCase))
        {
            return CategoriaSriActivoFijo.MaquinariaEquipo;
        }

        if (normalized.Contains("mueble", StringComparison.OrdinalIgnoreCase) || normalized.Contains("enser", StringComparison.OrdinalIgnoreCase))
        {
            return CategoriaSriActivoFijo.MueblesEnseres;
        }

        return CategoriaSriActivoFijo.EquiposComputo;
    }
}
