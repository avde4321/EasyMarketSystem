namespace TestDeIa.Application.Common;

public interface ITenantContextAccessor
{
    Guid? EmpresaId { get; set; }
    Guid? UserId { get; set; }
    bool IsSystemContext { get; set; }
}
