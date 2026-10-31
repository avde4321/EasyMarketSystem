namespace TestDeIa.Api.Controllers;

internal static class QueryDefaults
{
    public const int DefaultTake = 10;
    public const int MaxTake = 50;

    public static (int Skip, int Take) NormalizePaging(int skip, int take)
    {
        return (Math.Max(0, skip), Math.Clamp(take, 1, MaxTake));
    }

    public static (DateTimeOffset Desde, DateTimeOffset Hasta) ResolveCurrentMonthRange(
        DateTimeOffset? fechaDesde,
        DateTimeOffset? fechaHasta)
    {
        var now = DateTimeOffset.Now;
        var desde = fechaDesde ?? new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var hasta = fechaHasta ?? now;

        if (hasta < desde)
        {
            (desde, hasta) = (hasta, desde);
        }

        return (desde, hasta);
    }
}
