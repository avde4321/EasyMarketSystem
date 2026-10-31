namespace TestDeIa.Application.Modules.Security.Models;

public sealed record AuthenticatedUser(
    Guid Id,
    string UserName,
    string DisplayName,
    string Identification,
    string Email,
    Guid? DefaultEmpresaId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
