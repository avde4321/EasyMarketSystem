using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Tesoreria;
using TestDeIa.Shared.Requests.Tesoreria;
using TestDeIa.Shared.Responses.Tesoreria;

namespace TestDeIa.Client.Pages;

public partial class CuentasBancariasIndex
{
    [Inject] private TesoreriaApiClient TesoreriaApiClient { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private IReadOnlyCollection<CuentaBancariaDto> cuentas = Array.Empty<CuentaBancariaDto>();
    private CuentaBancariaDto? selectedCuenta;
    private CuentaBancariaRequest accountRequest = new();
    private string cuentaContableIdText = string.Empty;
    private string? errorMessage;
    private string? successMessage;
    private bool isLoading = true;
    private bool isSaving;
    private bool isAccountModalOpen;
    private bool isImportModalOpen;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        isLoading = true;
        errorMessage = null;
        cuentas = await TesoreriaApiClient.GetCuentasBancariasAsync();
        isLoading = false;
    }

    private void OpenCreateAccount()
    {
        accountRequest = new CuentaBancariaRequest
        {
            TipoCuenta = 1,
            Moneda = "USD",
            Activa = true
        };
        cuentaContableIdText = string.Empty;
        isAccountModalOpen = true;
    }

    private void CloseAccountModal()
    {
        isAccountModalOpen = false;
    }

    private async Task SaveAccountAsync()
    {
        if (!Guid.TryParse(cuentaContableIdText, out var cuentaContableId))
        {
            errorMessage = "Debes ingresar un Id válido de cuenta contable.";
            return;
        }

        isSaving = true;
        accountRequest.CuentaContableId = cuentaContableId;
        var result = await TesoreriaApiClient.SaveCuentaBancariaAsync(null, accountRequest);
        isSaving = false;

        if (!result.Succeeded)
        {
            errorMessage = result.ErrorMessage;
            return;
        }

        successMessage = "Cuenta bancaria registrada correctamente.";
        isAccountModalOpen = false;
        await LoadAsync();
    }

    private void OpenImportModal(CuentaBancariaDto cuenta)
    {
        selectedCuenta = cuenta;
        isImportModalOpen = true;
    }

    private void CloseImportModal()
    {
        isImportModalOpen = false;
    }

    private async Task HandleImportedAsync()
    {
        successMessage = "Extracto importado correctamente.";
        isImportModalOpen = false;
        await LoadAsync();
    }

    private void GoToConciliacion(Guid cuentaId)
    {
        NavigationManager.NavigateTo($"/tesoreria/conciliacion/{cuentaId}");
    }

    private static string MaskAccount(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= 4)
        {
            return value;
        }

        return $"**** {value[^4..]}";
    }
}
