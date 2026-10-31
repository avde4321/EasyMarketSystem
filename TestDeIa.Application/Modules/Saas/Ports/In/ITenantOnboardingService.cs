namespace TestDeIa.Application.Modules.Saas.Ports.In;

public interface ITenantOnboardingService
{
    Task<TestDeIa.Shared.Responses.Security.LoginResponse> RegistrarPublicoAsync(TestDeIa.Shared.Requests.Saas.PublicTenantRegisterDto dto, CancellationToken cancellationToken = default);
    Task AsignarSuscripcionInicialAsync(Guid empresaId, Guid administradorId, int? planInicialId, CancellationToken cancellationToken = default);
}
