namespace TestDeIa.Shared.Responses.Security;

public sealed class SecurityRoleResponse
{
    public string Name { get; set; } = string.Empty;

    public IReadOnlyCollection<string> Permissions { get; set; } = [];
}
