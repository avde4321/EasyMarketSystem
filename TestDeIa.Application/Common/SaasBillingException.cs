namespace TestDeIa.Application.Common;

public sealed class SaasBillingException(string message) : InvalidOperationException(message);
