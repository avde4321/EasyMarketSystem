namespace TestDeIa.Application.Common;

public sealed class SaaSQuotaExceededException : InvalidOperationException
{
    public SaaSQuotaExceededException(string message, string quotaCode)
        : base(message)
    {
        QuotaCode = quotaCode;
    }

    public string QuotaCode { get; }
}
