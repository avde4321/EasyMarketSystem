namespace TestDeIa.Application.Modules.Security.Models;

public sealed class PasswordVerificationResult
{
    public static readonly PasswordVerificationResult Failed = new(false, false);

    public PasswordVerificationResult(bool succeeded, bool requiresRehash)
    {
        Succeeded = succeeded;
        RequiresRehash = requiresRehash;
    }

    public bool Succeeded { get; }
    public bool RequiresRehash { get; }
}
