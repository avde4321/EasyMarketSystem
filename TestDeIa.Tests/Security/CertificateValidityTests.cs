using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;

namespace TestDeIa.Tests.Security;

public sealed class CertificateValidityTests
{
    [Theory]
    [InlineData(-10, -1, false)]
    [InlineData(1, 10, false)]
    [InlineData(-1, 10, true)]
    public void Validate_UsesCertificateValidity(int startDays, int endDays, bool valid)
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=Test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(startDays), DateTimeOffset.UtcNow.AddDays(endDays));
        if (valid)
            SriCertificateSelector.Validate(certificate);
        else
            Assert.Throws<InvalidOperationException>(() => SriCertificateSelector.Validate(certificate));
    }
}
