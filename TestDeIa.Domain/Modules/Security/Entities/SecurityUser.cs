namespace TestDeIa.Domain.Modules.Security.Entities;

public sealed class SecurityUser
{
    public SecurityUser(
        Guid id,
        string userName,
        string displayName,
        string email,
        string passwordHash,
        IReadOnlyCollection<string> roles,
        bool isActive)
    {
        Id = id;
        UserName = userName;
        DisplayName = displayName;
        Email = email;
        PasswordHash = passwordHash;
        Roles = roles;
        IsActive = isActive;
    }

    public Guid Id { get; }

    public string UserName { get; }

    public string DisplayName { get; }

    public string Email { get; }

    public string PasswordHash { get; }

    public IReadOnlyCollection<string> Roles { get; }

    public bool IsActive { get; }
}
