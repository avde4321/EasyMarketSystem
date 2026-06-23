namespace TestDeIa.Application.Modules.Security.Models;

public sealed record AuthenticatedUser(
    Guid Id,
    string UserName,
    string DisplayName,
    string Email,
    IReadOnlyCollection<string> Roles);
