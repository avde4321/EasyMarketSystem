namespace TestDeIa.Shared.Responses.Security;

using TestDeIa.Shared.Responses.Empresa;

public sealed class LoginResponse
{
    public bool Succeeded { get; set; }

    public string? Token { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public string? UserName { get; set; }

    public string? DisplayName { get; set; }

    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();

    public Guid? ActiveEmpresaId { get; set; }

    public IReadOnlyCollection<EmpresaOptionResponse> Empresas { get; set; } = Array.Empty<EmpresaOptionResponse>();

    public string? ErrorMessage { get; set; }
}
