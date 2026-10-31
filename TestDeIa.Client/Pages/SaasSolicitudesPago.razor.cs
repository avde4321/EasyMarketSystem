using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TestDeIa.Client.Services;
using TestDeIa.Client.Services.Saas;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Saas;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Client.Pages;

public partial class SaasSolicitudesPago : IDisposable
{
    [Inject] private SaasBillingApiClient Billing { get; set; } = default!;
    [Inject] private PopupNotificationService Notifications { get; set; } = default!;
    private readonly CancellationTokenSource lifetime = new();
    private IReadOnlyCollection<SaasPagoAdminResponse> items = [];
    private SaasPagoAdminResponse? selected;
    private string term = string.Empty, state = "Pendiente", notes = string.Empty;
    private DateOnly? from, until;
    private int skip, totalCount, pageSize = 20;
    private bool isLoading, isSending, bankVerified, focusModal;
    private string? errorMessage, infoMessage, decisionError;
    private ElementReference modalElement;
    protected override Task OnInitializedAsync() => LoadAsync(true);
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusModal && selected is not null) { focusModal = false; await modalElement.FocusAsync(); }
    }
    private static bool Pending(SaasPagoAdminResponse row) => row.EstadoPago == "Pendiente";
    private static string Badge(string status) => status switch { "Aprobado" => "approved", "Rechazado" => "rejected", _ => "pending" };
    private static string Money(decimal amount) => "USD $ " + amount.ToString("N2", CultureInfo.InvariantCulture);
    private static string LocalDate(DateTime? date) => date.HasValue
        ? SriFechaFiscal.EcuadorContinental(new DateTimeOffset(DateTime.SpecifyKind(date.Value, DateTimeKind.Utc))).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) : "—";
    private Task SearchAsync() => LoadAsync(true);
    private Task RefreshAsync() => LoadAsync(false);
    private async Task LoadAsync(bool reset)
    {
        if (isLoading || isSending) return;
        if (from > until) { errorMessage = "La fecha inicial no puede ser posterior a la final."; return; }
        if (reset) skip = 0;
        isLoading = true; errorMessage = null;
        try
        {
            var page = await Billing.GetAdminPaymentsAsync(new SaasPagosAdminQuery { Term = term, Estado = state, FechaDesde = from, FechaHasta = until, Skip = skip, Take = pageSize }, lifetime.Token);
            items = page.Items; totalCount = page.TotalCount;
            if (skip > 0 && items.Count == 0) { skip = 0; isLoading = false; await LoadAsync(false); }
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        catch (Exception ex) when (ex is SaasBillingClientException or HttpRequestException or JsonException)
        { errorMessage = ex is SaasBillingClientException ? ex.Message : "No se pudieron consultar las solicitudes. Intenta nuevamente."; }
        finally { isLoading = false; }
    }
    private Task PreviousAsync() { skip = Math.Max(0, skip - pageSize); return LoadAsync(false); }
    private Task NextAsync() { if (skip + pageSize < totalCount) skip += pageSize; return LoadAsync(false); }
    private void OpenReview(SaasPagoAdminResponse row) { selected = row; notes = string.Empty; bankVerified = false; decisionError = null; focusModal = true; }
    private void CloseReview() { if (!isSending) { selected = null; decisionError = null; } }
    private void OnModalKeyDown(KeyboardEventArgs e) { if (e.Key == "Escape") CloseReview(); }
    private async Task DecideAsync(bool approve)
    {
        if (isSending || selected is null || !Pending(selected)) return;
        if (approve && !bankVerified) { decisionError = "Confirma primero la verificación bancaria."; return; }
        if (!approve && string.IsNullOrWhiteSpace(notes)) { decisionError = "Indica el motivo del rechazo."; return; }
        var payment = selected;
        isSending = true; decisionError = null;
        try
        {
            var result = await Billing.ConfirmPaymentAsync(new ConfirmarPagoSaasDto { PagoId = payment.Id, EmpresaId = payment.EmpresaId, Aprobar = approve, Observaciones = notes.Trim() }, lifetime.Token);
            infoMessage = result.EstadoPago == "Aprobado" ? "Pago aprobado. El plan fue habilitado o renovado correctamente." : "Pago rechazado. La suscripción no fue modificada.";
            selected = null;
            await Notifications.ShowSuccessAsync(infoMessage);
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        catch (Exception ex) when (ex is SaasBillingClientException or HttpRequestException or JsonException)
        { decisionError = ex is SaasBillingClientException ? ex.Message : "No se pudo confirmar la decisión. Consulta el estado antes de reintentar."; }
        finally { isSending = false; }
        if (selected is null && !lifetime.IsCancellationRequested) await LoadAsync(false);
    }
    public void Dispose() { lifetime.Cancel(); lifetime.Dispose(); }
}
