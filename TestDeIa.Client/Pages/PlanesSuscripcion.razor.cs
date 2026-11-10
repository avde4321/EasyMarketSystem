using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TestDeIa.Client.Services;
using TestDeIa.Client.Services.Saas;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Saas;

namespace TestDeIa.Client.Pages;

public partial class PlanesSuscripcion : IDisposable
{
    [Inject] private SaasApiClient Saas { get; set; } = default!;
    [Inject] private SaasBillingApiClient Billing { get; set; } = default!;
    [Inject] private PopupNotificationService Notifications { get; set; } = default!;

    private readonly CancellationTokenSource lifetime = new();
    private IReadOnlyCollection<SaasPlanResponse> plans = [];
    private SaasBillingPortalResponse? portal;
    private SaasPlanResponse? selectedPlan;
    private readonly TransferForm transfer = new();
    private RequestMode mode = RequestMode.PlanChange;
    private int periodMonths = 1;
    private bool isLoading = true, isSending, showTransfer, focusModal;
    private string? errorMessage, infoMessage, transferError;
    private ElementReference modalElement;
    private bool HasPending => portal?.PagosPendientes.Count > 0;
    private decimal SelectedTotal => (selectedPlan?.PrecioMensual ?? 0m) * periodMonths;
    private static DateOnly TodayEcuador => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5));
    private static string Money(decimal amount) => "USD $ " + amount.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
    private static string Limit(int amount) => amount == 0 ? "Sin límite" : amount.ToString(CultureInfo.InvariantCulture);
    private string PlanName(int id) => plans.FirstOrDefault(x => x.Id == id)?.Nombre ?? "Plan registrado";
    private IEnumerable<SaasPlanResponse> VisiblePlans => mode == RequestMode.AdvancePayment && portal?.SaasPlanId is int current
        ? plans.Where(x => x.Id == current) : plans;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        if (isSending) return;
        isLoading = true;
        errorMessage = null;
        try
        {
            plans = (await Saas.GetPlansAsync(lifetime.Token)).Where(x => x.EsActivo).ToArray();
            portal = await Billing.GetPortalAsync(lifetime.Token);
            if (portal.SaasPlanId is int planId)
                selectedPlan = plans.FirstOrDefault(x => x.Id == planId);
            if (HasPending) selectedPlan = null;
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or SaasBillingClientException)
        { if (!lifetime.IsCancellationRequested) errorMessage = ex is SaasBillingClientException ? ex.Message : "No se pudo consultar la suscripción. Intenta nuevamente."; }
        finally { isLoading = false; }
    }

    private void SetMode(RequestMode value)
    {
        if (HasPending || isSending) return;
        mode = value;
        periodMonths = 1;
        selectedPlan = value == RequestMode.AdvancePayment && portal?.SaasPlanId is int id
            ? plans.FirstOrDefault(x => x.Id == id)
            : null;
        infoMessage = null;
    }

    private void SelectPlan(SaasPlanResponse plan)
    {
        if (!HasPending && !isSending && plan.PrecioMensual > 0
            && (mode != RequestMode.AdvancePayment || plan.Id == portal?.SaasPlanId))
        {
            selectedPlan = plan;
            infoMessage = null;
        }
    }

    private void OpenTransfer()
    {
        if (selectedPlan is null || HasPending || isSending || SelectedTotal <= 0) return;
        transfer.FechaPago = TodayEcuador;
        transfer.ComprobanteReferencia = string.Empty;
        transfer.Observaciones = null;
        transferError = null;
        showTransfer = true;
        focusModal = true;
    }

    private void CloseTransfer() { if (!isSending) showTransfer = false; }
    private void OnModalKeyDown(KeyboardEventArgs e) { if (e.Key == "Escape") CloseTransfer(); }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusModal && showTransfer) { focusModal = false; await modalElement.FocusAsync(); }
    }

    private async Task SubmitTransferAsync()
    {
        if (isSending || selectedPlan is null || HasPending) return;
        if (mode == RequestMode.AdvancePayment && selectedPlan.Id != portal?.SaasPlanId)
        { transferError = "El prepago solo puede aplicarse al plan vigente."; return; }
        isSending = true;
        transferError = null;
        try
        {
            var result = await Billing.RegisterTransferAsync(new RegistrarTransferenciaSaasDto
            {
                SaasPlanId = selectedPlan.Id,
                PeriodoMeses = periodMonths,
                Monto = SelectedTotal,
                FechaPago = transfer.FechaPago,
                ComprobanteReferencia = transfer.ComprobanteReferencia,
                Observaciones = transfer.Observaciones
            }, lifetime.Token);
            portal = new SaasBillingPortalResponse(
                portal?.SuscripcionActiva ?? false,
                portal?.EstadoSuscripcion,
                portal?.FechaVencimiento,
                result.EstadoPago == "Pendiente" ? [result] : [],
                result,
                portal?.SaasPlanId,
                portal?.PlanNombre);
            infoMessage = $"Solicitud registrada por {result.PeriodoMeses} mes(es). Quedará pendiente hasta que se verifique el pago.";
            showTransfer = false;
            await Notifications.ShowSuccessAsync(infoMessage);
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or SaasBillingClientException)
        {
            if (!lifetime.IsCancellationRequested)
            {
                transferError = ex is SaasBillingClientException ? ex.Message : "No se pudo confirmar el envío. Actualiza el estado antes de reintentar.";
                await Notifications.ShowErrorAsync(transferError);
            }
        }
        finally { isSending = false; }
    }

    private enum RequestMode { PlanChange, AdvancePayment }

    private sealed class TransferForm : IValidatableObject
    {
        [Required(ErrorMessage = "Ingresa la referencia del comprobante."), StringLength(200)]
        public string ComprobanteReferencia { get; set; } = string.Empty;
        [Required(ErrorMessage = "Selecciona la fecha del pago.")]
        public DateOnly? FechaPago { get; set; }
        [StringLength(2000)] public string? Observaciones { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaPago > TodayEcuador)
                yield return new ValidationResult("La fecha no puede ser posterior a hoy.", [nameof(FechaPago)]);
        }
    }

    public void Dispose() { lifetime.Cancel(); lifetime.Dispose(); }
}
