using TestDeIa.Shared.Sri;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;

namespace TestDeIa.Tests.Sri;

public sealed class SriFechaFiscalTests
{
    [Theory]
    [InlineData("2026-10-30T04:56:57+00:00", "29/10/2026", "29102026")]
    [InlineData("2026-10-30T05:00:00+00:00", "30/10/2026", "30102026")]
    public void FiscalDateAndAccessKeyUseSameEcuadorDay(string instant, string xmlDate, string keyDate)
    {
        var utc = DateTimeOffset.Parse(instant, System.Globalization.CultureInfo.InvariantCulture);
        var fiscal = SriFechaFiscal.EcuadorContinental(utc);
        Assert.Equal(utc.UtcDateTime, fiscal.UtcDateTime);
        Assert.Equal(xmlDate, SriFechaFiscal.Formatear(fiscal));
        var key = new ClaveAccesoService().Generar(fiscal, "01", "0999999999001", "1", "001", "001", "000000001", "12345678", "1");
        Assert.StartsWith(keyDate, key);
    }
}
