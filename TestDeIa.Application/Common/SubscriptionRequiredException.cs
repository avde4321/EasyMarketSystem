namespace TestDeIa.Application.Common;

public sealed class SubscriptionRequiredException : InvalidOperationException
{
    public const string DefaultCode = "SUBSCRIPTION_REQUIRED";

    public SubscriptionRequiredException(string message)
        : base(message)
    {
    }

    public string Code => DefaultCode;
}
