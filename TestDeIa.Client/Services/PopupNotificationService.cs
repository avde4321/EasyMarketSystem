using Microsoft.JSInterop;

namespace TestDeIa.Client.Services;

public sealed class PopupNotificationService(IJSRuntime jsRuntime)
{
    public ValueTask ShowSuccessAsync(string message) => ShowAsync(message, "success");

    public ValueTask ShowErrorAsync(string message) => ShowAsync(message, "error");

    public ValueTask ShowInfoAsync(string message) => ShowAsync(message, "info");

    private async ValueTask ShowAsync(string? message, string variant)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        try
        {
            await jsRuntime.InvokeVoidAsync("easyMarketPopups.show", message.Trim(), variant);
        }
        catch (JSException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }
}
