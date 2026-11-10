using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Application.Modules.Empresa.Ports.In;

public interface IEmpresaSmtpConfiguracionService
{
    Task<EmpresaSmtpConfiguracionResponse?> ObtenerAsync(CancellationToken cancellationToken);
    Task<EmpresaSmtpConfiguracionResponse> GuardarAsync(EmpresaSmtpConfiguracionRequest request, CancellationToken cancellationToken);
}

public interface IEmpresaSmtpCredentialProvider
{
    Task<EmpresaSmtpCredentials?> ObtenerCredencialesAsync(Guid empresaId, CancellationToken cancellationToken);
}

public sealed record EmpresaSmtpCredentials(
    Guid EmpresaId,
    string Host,
    int Puerto,
    bool UsarSsl,
    bool RequiereAutenticacion,
    string? Usuario,
    string? Contrasena,
    string CorreoRemitente,
    string NombreRemitente,
    bool Habilitado);
