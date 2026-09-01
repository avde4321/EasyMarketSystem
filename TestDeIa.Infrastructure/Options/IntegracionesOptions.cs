namespace TestDeIa.Infrastructure.Options;

public sealed class IntegracionesOptions
{
    public const string SectionName = "Integraciones";

    public WhatsAppCloudApiOptions WhatsApp { get; set; } = new();

    public PayPhoneOptions PayPhone { get; set; } = new();
}

public sealed class WhatsAppCloudApiOptions
{
    public string GraphBaseUrl { get; set; } = "https://graph.facebook.com";

    public string ApiVersion { get; set; } = "v20.0";

    public string DefaultCountryCode { get; set; } = "593";

    public string ComprobanteTemplateName { get; set; } = "easy_market_comprobante";

    public string RecordatorioPagoTemplateName { get; set; } = "easy_market_recordatorio_pago";

    public string LanguageCode { get; set; } = "es";
}

public sealed class PayPhoneOptions
{
    public string PruebasBaseUrl { get; set; } = "https://pay.payphonetodoesposible.com";

    public string ProduccionBaseUrl { get; set; } = "https://pay.payphonetodoesposible.com";

    public string WebhookTokenHeaderName { get; set; } = "X-PayPhone-Token";
}
