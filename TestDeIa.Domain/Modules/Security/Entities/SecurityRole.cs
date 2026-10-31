namespace TestDeIa.Domain.Modules.Security.Entities;

public sealed class SecurityRole
{
    public SecurityRole(Guid id, string name, bool isActive, IReadOnlyCollection<string>? permissions = null)
    {
        Id = id;
        Name = name;
        IsActive = isActive;
        Permissions = permissions ?? [];
    }

    public Guid Id { get; }

    public string Name { get; }

    public bool IsActive { get; }

    public IReadOnlyCollection<string> Permissions { get; }
}
