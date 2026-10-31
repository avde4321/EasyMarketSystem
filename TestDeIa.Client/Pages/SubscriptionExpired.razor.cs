using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using TestDeIa.Client.Services;
using TestDeIa.Client.Services.Saas;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Saas;
using TestDeIa.Shared.Security;

namespace TestDeIa.Client.Pages;

public partial class SubscriptionExpired : IDisposable
{
    [Inject] private SaasApiClient Saas { get; set; } = default!;
    [Inject] private SaasBillingApiClient Billing { get; set; } = default!;
    [Inject] private AuthenticationStateProvider Authentication { get; set; } = default!;
    [Inject] private PopupNotificationService Notifications { get; set; } = default!;
    private readonly CancellationTokenSource lifetime = new();
    private IReadOnlyCollection<SaasPlanResponse> plans = [];
    private SaasBillingPortalResponse? portal;
    private SaasPlanResponse? selectedPlan;
    private TransferForm transferForm = new();
    private bool isLoading = true, isSending, canManage, portalLoaded, showTransfer, focusModal;
    private string? errorMessage, transferError, infoMessage;
    private ElementReference modalElement;
    private bool HasPending => portal?.PagosPendientes.Count > 0;
    private static DateOnly TodayEcuador => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5));
    private static string Money(decimal amount) => "USD $ " + amount.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
    private static string Limit(int value) => value == 0 ? "Sin límite" : value.ToString();
    private string PlanName(int id) => plans.FirstOrDefault(x => x.Id == id)?.Nombre ?? "Plan registrado";

    protected override async Task OnInitializedAsync()
    {
        var user = (await Authentication.GetAuthenticationStateAsync()).User;
        canManage = user.HasClaim(SecurityClaimTypes.Permission, SecurityPermissions.EmpresaConfigurar);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (isSending) return;
        isLoading = true; portalLoaded = false; errorMessage = null;
        try
        {
            plans = (await Saas.GetPlansAsync(lifetime.Token)).Where(x => x.EsActivo).ToArray();
            portal = await Billing.GetPortalAsync(lifetime.Token);
            portalLoaded = true;
            if (HasPending) { showTransfer = false; selectedPlan = null; }
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException or SaasBillingClientException)
        { if (!lifetime.IsCancellationRequested) errorMessage = ex is SaasBillingClientException ? ex.Message : "No se pudo consultar el estado. Actualiza antes de registrar un pago para evitar duplicados."; }
        finally { isLoading = false; }
    }

    private void SelectPlan(SaasPlanResponse plan) { if (!HasPending && !isSending && canManage && portalLoaded) { selectedPlan = plan; infoMessage = null; } }
    private void OpenTransfer()
    {
        if (selectedPlan is null || HasPending || isSending || !canManage || !portalLoaded) return;
        transferForm = new TransferForm { FechaPago = TodayEcuador };
        transferError = null; showTransfer = true; focusModal = true;
    }
    private void CloseTransfer() { if (!isSending) showTransfer = false; }
    private void OnModalKeyDown(KeyboardEventArgs e) { if (e.Key == "Escape") CloseTransfer(); }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    { if (focusModal && showTransfer) { focusModal = false; await modalElement.FocusAsync(); } }

    private async Task SendTransferAsync()
    {
        if (isSending || selectedPlan is null || HasPending || !canManage || !portalLoaded) return;
        isSending = true; transferError = null;
        try
        {
            var payment = await Billing.RegisterTransferAsync(new RegistrarTransferenciaSaasDto { SaasPlanId = selectedPlan.Id,
                Monto = selectedPlan.PrecioMensual, ComprobanteReferencia = transferForm.Referencia, FechaPago = transferForm.FechaPago,
                Observaciones = transferForm.Observaciones }, lifetime.Token);
            portal = new SaasBillingPortalResponse(portal?.SuscripcionActiva ?? false, portal?.EstadoSuscripcion, portal?.FechaVencimiento,
                payment.EstadoPago == "Pendiente" ? [payment] : [], payment);
            showTransfer = false;
            infoMessage = payment.EstadoPago == "Pendiente" ? "Comprobante registrado correctamente. Está pendiente de verificación." : "Esta referencia ya estaba registrada. Actualiza para revisar el estado actual.";
            await Notifications.ShowSuccessAsync(infoMessage);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException or SaasBillingClientException)
        {
            if (!lifetime.IsCancellationRequested)
            {
                transferError = ex is SaasBillingClientException ? ex.Message : "No se pudo confirmar el envío. Cierra el formulario y actualiza el estado antes de volver a registrar la misma referencia.";
                await Notifications.ShowErrorAsync(transferError);
            }
        }
        finally { isSending = false; }
    }

    private async Task CheckoutAsync()
    {
        if (selectedPlan is null || isSending || HasPending || !canManage || !portalLoaded) return;
        isSending = true; errorMessage = null;
        try
        {
            var result = await Billing.CheckoutAsync(selectedPlan.Id, lifetime.Token);
            infoMessage = result.Mensaje;
            await Notifications.ShowInfoAsync(infoMessage);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException or SaasBillingClientException)
        { if (!lifetime.IsCancellationRequested) errorMessage = ex is SaasBillingClientException ? ex.Message : "No se pudo consultar la pasarela. No se ha registrado ningún cobro desde esta pantalla."; }
        finally { isSending = false; }
    }

    private sealed class TransferForm : IValidatableObject
    {
        [Required(ErrorMessage = "Ingresa la referencia del depósito o transferencia."), StringLength(200)] public string Referencia { get; set; } = string.Empty;
        [Required(ErrorMessage = "Selecciona la fecha del pago.")] public DateOnly? FechaPago { get; set; }
        [StringLength(2000)] public string? Observaciones { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaPago > TodayEcuador) yield return new ValidationResult("La fecha no puede ser posterior a hoy.", [nameof(FechaPago)]);
        }
    }
    public void Dispose() { lifetime.Cancel(); lifetime.Dispose(); }
}
