namespace TestDeIa.Shared.Responses.Security;

public sealed class LoginResponse
{
    public bool Succeeded { get; set; }

    public string? Token { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public string? UserName { get; set; }

    public string? DisplayName { get; set; }

    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();

    public string? ErrorMessage { get; set; }
}
