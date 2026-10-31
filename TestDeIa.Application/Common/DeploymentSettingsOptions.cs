namespace TestDeIa.Application.Common;

/// <summary>Política del servidor; nunca se acepta desde el navegador o el JWT.</summary>
public sealed class DeploymentSettingsOptions
{
    public const string SectionName = "DeploymentSettings";
    public string Mode { get; set; } = "SaaS";
    public bool EnforceSubscription { get; set; } = true;
    public bool EnforceQuotas { get; set; } = true;
    public Guid? OnPremiseEmpresaId { get; set; }
    public bool IsOnPremise => string.Equals(Mode, "OnPremise", StringComparison.OrdinalIgnoreCase);
    public bool SubscriptionEnabled => !IsOnPremise && EnforceSubscription;
    public bool QuotasEnabled => !IsOnPremise && EnforceQuotas;
}
