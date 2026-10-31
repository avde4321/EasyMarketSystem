namespace TestDeIa.Domain.Modules.Saas.Services;

public static class SaasConsumptionAnalysis
{
    public sealed record Result(decimal? Usage, decimal? Growth, int Forecast, bool SuggestUpgrade, string Reason);
    public sealed record Capacity(int Invoices, int Users, int Branches, bool Sri, bool AdvancedKardex);

    public static bool CanUpgrade(Capacity current, Capacity candidate, long requiredInvoices)
        => current.Invoices > 0
           && (candidate.Invoices == 0 || candidate.Invoices > current.Invoices && candidate.Invoices >= requiredInvoices)
           && (candidate.Users == 0 || current.Users > 0 && candidate.Users >= current.Users)
           && (candidate.Branches == 0 || current.Branches > 0 && candidate.Branches >= current.Branches)
           && (!current.Sri || candidate.Sri) && (!current.AdvancedKardex || candidate.AdvancedKardex);

    public static Result Evaluate(int used, int reserved, int limit, int authorized, int previousMonth, int elapsedDays, int daysInMonth)
    {
        if (new[] { used, reserved, limit, authorized, previousMonth }.Any(x => x < 0)
            || elapsedDays < 1 || elapsedDays > daysInMonth || daysInMonth is < 28 or > 31)
            throw new ArgumentOutOfRangeException(nameof(used));
        var usage = limit == 0 ? (decimal?)null : Math.Round(((decimal)used + reserved) * 100 / limit, 2);
        var forecast = (int)Math.Min(int.MaxValue, decimal.Ceiling((decimal)authorized * daysInMonth / elapsedDays));
        var growth = previousMonth == 0 || elapsedDays < 7 ? (decimal?)null
            : Math.Round(((decimal)forecast - previousMonth) * 100 / previousMonth, 2);
        if (limit == 0) return new(usage, growth, forecast, false, "Plan sin límite mensual de facturas.");
        if ((long)used + reserved >= limit) return new(usage, growth, forecast, true, "Cupo contratado ocupado por consumo y reservas.");
        if (usage >= 80) return new(usage, growth, forecast, true, "Consumo más reservas alcanza al menos el 80% del cupo.");
        if (elapsedDays >= 7 && forecast > limit) return new(usage, growth, forecast, true, "El ritmo de autorizaciones proyecta superar el límite mensual.");
        return new(usage, growth, forecast, false, elapsedDays < 7
            ? "Datos iniciales: se requieren 7 días para sugerir por tendencia."
            : "El consumo observado se mantiene dentro del plan.");
    }
}
