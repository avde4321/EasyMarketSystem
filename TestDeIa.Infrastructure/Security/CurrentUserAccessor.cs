using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Application.Common;

namespace TestDeIa.Infrastructure.Security;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly ITenantContextAccessor tenantContextAccessor;

    public CurrentUserAccessor(ITenantContextAccessor tenantContextAccessor)
    {
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public Guid? GetUserId()
    {
        return tenantContextAccessor.UserId;
    }

    public Guid GetRequiredUserId()
    {
        return tenantContextAccessor.UserId
            ?? throw new InvalidOperationException("No existe un usuario autenticado en el contexto actual.");
    }
}
