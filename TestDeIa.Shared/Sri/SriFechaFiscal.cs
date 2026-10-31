using System.Globalization;

namespace TestDeIa.Shared.Sri;

public static class SriFechaFiscal
{
    // Ecuador continental (domicilio fiscal de esta implementación), independiente del host.
    public static DateTimeOffset EcuadorContinental(DateTimeOffset instante)
        => instante.ToOffset(TimeSpan.FromHours(-5));

    public static string Formatear(DateTimeOffset fechaFiscal)
        => fechaFiscal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}
