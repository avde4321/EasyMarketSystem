using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Caja;
using TestDeIa.Shared.Requests.Caja;
using TestDeIa.Shared.Responses.Caja;

namespace TestDeIa.Client.Pages;

public partial class ControlCaja
{
    [Inject]
    private CajaApiClient CajaApiClient { get; set; } = default!;

    private CajaSesionResponse? cajaActiva;
    private decimal montoApertura;
    private decimal montoFisicoEfectivoReal;
    private decimal montoFisicoTarjetaReal;
    private bool isLoading = true;
    private bool isSaving;
    private string? errorMessage;
    private string? statusMessage;

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            cajaActiva = await CajaApiClient.GetActivaAsync();
            if (cajaActiva is not null)
            {
                montoFisicoEfectivoReal = cajaActiva.MontoFisicoEfectivoReal;
                montoFisicoTarjetaReal = cajaActiva.MontoFisicoTarjetaReal;
            }
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo consultar el estado de la caja.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task AbrirCajaAsync()
    {
        errorMessage = null;
        statusMessage = null;
        isSaving = true;

        try
        {
            var result = await CajaApiClient.AbrirAsync(new AbrirCajaRequest
            {
                MontoApertura = montoApertura
            });

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            cajaActiva = result.Data;
            statusMessage = "La caja quedo abierta y el POS ya puede operar.";
            montoFisicoEfectivoReal = 0;
            montoFisicoTarjetaReal = 0;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo abrir la caja.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task CerrarCajaAsync()
    {
        errorMessage = null;
        statusMessage = null;
        isSaving = true;

        try
        {
            var result = await CajaApiClient.CerrarAsync(new CerrarCajaRequest
            {
                MontoFisicoEfectivoReal = montoFisicoEfectivoReal,
                MontoFisicoTarjetaReal = montoFisicoTarjetaReal
            });

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            cajaActiva = null;
            montoApertura = 0;
            montoFisicoEfectivoReal = 0;
            montoFisicoTarjetaReal = 0;
            statusMessage = "La caja se cerro correctamente y el arqueo quedo conciliado.";
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cerrar la caja.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private static string GetDifferenceClass(decimal difference)
    {
        if (difference == 0)
        {
            return "neutral";
        }

        return difference > 0 ? "positive" : "negative";
    }
}
