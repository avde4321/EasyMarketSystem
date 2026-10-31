using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TestDeIa.Client.Security;
using TestDeIa.Client.Services.Empresa;
using TestDeIa.Client.Services.Saas;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Saas;

namespace TestDeIa.Client.Pages;

public partial class RegisterTenant : IDisposable
{
    [Inject] private PublicTenantApiClient PublicApi { get; set; } = default!;
    [Inject] private TokenAuthenticationStateProvider Authentication { get; set; } = default!;
    [Inject] private EmpresaSessionService EmpresaSession { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    private readonly PublicTenantRegisterDto request = new() { EsDemo = true };
    private readonly CancellationTokenSource lifetime = new();
    private IReadOnlyCollection<SaasPlanResponse> plans = [];
    private bool isLoadingPlans = true;
    private bool isBusy;
    private bool registrationCompleted;
    private string? errorMessage;
    private string? plansError;
    private SaasPlanResponse? SelectedPlan => plans.FirstOrDefault(x => x.Id == request.PlanId);
    private static string Limit(int value) => value == 0 ? "Sin límite" : value.ToString();
    private static string Price(SaasPlanResponse plan) => plan.PrecioMensual == 0 ? "Precio personalizado" : $"USD {plan.PrecioMensual.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}/mes";

    protected override Task OnInitializedAsync() => LoadPlansAsync();

    private async Task LoadPlansAsync()
    {
        isLoadingPlans = true;
        plansError = null;
        try
        {
            plans = (await PublicApi.GetPlansAsync(lifetime.Token)).Where(x => x.EsActivo).ToArray();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            if (!lifetime.IsCancellationRequested) plansError = "No se pudieron cargar los planes. Revisa tu conexión e intenta nuevamente.";
        }
        finally { isLoadingPlans = false; }
    }

    private async Task RegisterAsync()
    {
        if (isBusy || registrationCompleted || SelectedPlan is null) return;
        isBusy = true;
        errorMessage = null;
        try
        {
            var response = await PublicApi.RegisterAsync(request, lifetime.Token);
            if (!response.Succeeded)
            {
                errorMessage = response.ErrorMessage ?? "No se pudo registrar la empresa. Revisa los datos ingresados.";
                return;
            }
            registrationCompleted = true;
            request.Password = string.Empty;
            if (string.IsNullOrWhiteSpace(response.Token) || !response.ActiveEmpresaId.HasValue)
            {
                errorMessage = "Tu registro fue recibido, pero no se pudo iniciar la sesión. Ingresa desde el inicio de sesión con tu correo y contraseña.";
                return;
            }
            await EmpresaSession.SetEmpresaIdAsync(response.ActiveEmpresaId.Value);
            await Authentication.MarkUserAsAuthenticatedAsync(response.Token);
            Navigation.NavigateTo("/dashboard", replace: true);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            if (!lifetime.IsCancellationRequested)
                errorMessage = "No se pudo confirmar el registro. Si ya se creó tu cuenta, inicia sesión con tu correo antes de intentar registrarla nuevamente.";
        }
        catch (JSException)
        {
            errorMessage = "Tu empresa fue registrada, pero el navegador no pudo guardar la sesión. Habilita el almacenamiento e inicia sesión con tu correo.";
        }
        finally { isBusy = false; }
    }

    public void Dispose() { lifetime.Cancel(); lifetime.Dispose(); }
}
