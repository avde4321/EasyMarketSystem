using System.Text;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;

namespace TestDeIa.Tests.Sri;

public sealed class SriEnvironmentTests
{
    [Theory]
    [InlineData("1", "1", "1", true)]
    [InlineData("2", "2", "2", true)]
    [InlineData("1", "2", "1", false)]
    [InlineData("2", "1", "1", false)]
    [InlineData("1", "1", "2", false)]
    public void EnvironmentMustMatchXmlAndAccessKey(string request, string xml, string key, bool valid)
    {
        var accessKey = new string('1', 23) + key + new string('1', 25);
        var bytes = Encoding.UTF8.GetBytes($"<factura><infoTributaria><ambiente>{xml}</ambiente><claveAcceso>{accessKey}</claveAcceso></infoTributaria></factura>");
        if (valid) SriSoapClient.ValidateEnvironment(request, bytes);
        else Assert.Throws<InvalidOperationException>(() => SriSoapClient.ValidateEnvironment(request, bytes));
    }
}
