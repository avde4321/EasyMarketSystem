namespace TestDeIa.Domain.Modules.Security.Entities;

public sealed class SecurityRole
{
    public SecurityRole(Guid id, string name, bool isActive)
    {
        Id = id;
        Name = name;
        IsActive = isActive;
    }

    public Guid Id { get; }

    public string Name { get; }

    public bool IsActive { get; }
}
