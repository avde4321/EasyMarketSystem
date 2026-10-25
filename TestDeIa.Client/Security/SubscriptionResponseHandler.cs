using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace TestDeIa.Client.Security;

public sealed class SubscriptionResponseHandler(NavigationManager navigationManager) : DelegatingHandler
{
    private const string SubscriptionRequiredCode = "SUBSCRIPTION_REQUIRED";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode is HttpStatusCode.PaymentRequired or HttpStatusCode.Forbidden &&
            await IsSubscriptionRequiredAsync(response, cancellationToken))
        {
            navigationManager.NavigateTo("/subscription-expired");
        }

        return response;
    }

    private static async Task<bool> IsSubscriptionRequiredAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentType?.MediaType != "application/problem+json")
        {
            return false;
        }

        var contentType = response.Content.Headers.ContentType;
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        response.Content = new StringContent(payload);
        response.Content.Headers.ContentType = contentType;

        try
        {
            using var document = JsonDocument.Parse(payload);

            if (document.RootElement.TryGetProperty("code", out var code) &&
                string.Equals(code.GetString(), SubscriptionRequiredCode, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (document.RootElement.TryGetProperty("extensions", out var extensions) &&
                extensions.TryGetProperty("code", out var extensionCode))
            {
                return string.Equals(extensionCode.GetString(), SubscriptionRequiredCode, StringComparison.OrdinalIgnoreCase);
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }
}
