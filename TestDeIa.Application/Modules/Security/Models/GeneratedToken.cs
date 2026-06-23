namespace TestDeIa.Application.Modules.Security.Models;

public sealed record GeneratedToken(
    string AccessToken,
    DateTimeOffset ExpiresAt);
