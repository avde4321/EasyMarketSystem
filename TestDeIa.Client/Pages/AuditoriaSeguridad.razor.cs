using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Security;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Client.Pages;

public partial class AuditoriaSeguridad
{
    [Inject]
    private SecurityApiClient SecurityApiClient { get; set; } = default!;

    private readonly List<SecurityAuditLogResponse> auditLogs = [];
    private string searchTerm = string.Empty;
    private string? errorMessage;
    private bool isLoading = true;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;

    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync(true);
    }

    private async Task LoadAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var page = await SecurityApiClient.GetAuditLogsAsync(searchTerm, currentSkip, PageSize);
            auditLogs.Clear();
            auditLogs.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar la auditoria de seguridad.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private Task SearchAsync() => LoadAsync(true);

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadAsync();
    }

    private static string FormatEvent(string eventName)
        => eventName.Replace("Administrative", " administrativo", StringComparison.Ordinal)
            .Replace("Login", "Login ", StringComparison.Ordinal)
            .Replace("Cambio", "Cambio ", StringComparison.Ordinal)
            .Replace("Usuario", " usuario", StringComparison.Ordinal);

    private static string GetAuditClass(string eventName)
        => eventName switch
        {
            "LoginExitoso" => "audit-pill-success",
            "LoginFallido" => "audit-pill-warning",
            "BloqueoUsuario" or "CambioEstado" => "audit-pill-danger",
            _ => "audit-pill-soft"
        };
}
