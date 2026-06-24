namespace TestDeIa.Application.Modules.Security.Ports.Out;

public interface ICurrentUserAccessor
{
    Guid? GetUserId();
    Guid GetRequiredUserId();
}
