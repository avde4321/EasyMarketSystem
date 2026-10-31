using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services;
using TestDeIa.Client.Services.Caja;
using TestDeIa.Shared.Requests.Caja;
using TestDeIa.Shared.Responses.Caja;

namespace TestDeIa.Client.Pages;

public partial class ControlCaja
{
    [Inject]
    private CajaApiClient CajaApiClient { get; set; } = default!;

    [Inject]
    private PopupNotificationService PopupNotificationService { get; set; } = default!;

    private CajaSesionResponse? cajaActiva;
    private decimal montoApertura;
    private decimal montoFisicoEfectivoReal;
    private decimal montoFisicoTarjetaReal;
    private decimal montoFisicoTransferenciaReal;
    private bool isLoading = true;
    private bool isSaving;
    private string? errorMessage;
    private string? statusMessage;

    private decimal EfectivoEsperado => cajaActiva is null
        ? 0m
        : Math.Round(cajaActiva.MontoApertura + cajaActiva.TotalVentasEfectivoCalculado, 2);

    private decimal DiferenciaEfectivoPreview => Math.Round(montoFisicoEfectivoReal - EfectivoEsperado, 2);

    private decimal DiferenciaTarjetaPreview => cajaActiva is null
        ? 0m
        : Math.Round(montoFisicoTarjetaReal - cajaActiva.TotalVentasTarjetaCalculado, 2);

    private decimal DiferenciaTransferenciaPreview => cajaActiva is null
        ? 0m
        : Math.Round(montoFisicoTransferenciaReal - cajaActiva.TotalVentasTransferenciaCalculado, 2);

    private decimal DiferenciaTotalPreview => Math.Round(
        DiferenciaEfectivoPreview + DiferenciaTarjetaPreview + DiferenciaTransferenciaPreview,
        2);

    private string DifferenceLabel => DiferenciaTotalPreview switch
    {
        > 0m => "Sobrante total",
        < 0m => "Faltante total",
        _ => "Caja cuadrada"
    };

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
                montoFisicoTransferenciaReal = cajaActiva.MontoFisicoTransferenciaReal;
            }
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo consultar el estado de la caja.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
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
                errorMessage = result.ErrorMessage ?? "No se pudo abrir la caja.";
                await PopupNotificationService.ShowErrorAsync(errorMessage);
                return;
            }

            cajaActiva = result.Data;
            statusMessage = "La caja quedo abierta y el POS ya puede operar.";
            await PopupNotificationService.ShowSuccessAsync(statusMessage);
            montoFisicoEfectivoReal = 0;
            montoFisicoTarjetaReal = 0;
            montoFisicoTransferenciaReal = 0;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo abrir la caja.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
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
                MontoFisicoTarjetaReal = montoFisicoTarjetaReal,
                MontoFisicoTransferenciaReal = montoFisicoTransferenciaReal
            });

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage ?? "No se pudo cerrar la caja.";
                await PopupNotificationService.ShowErrorAsync(errorMessage);
                return;
            }

            cajaActiva = null;
            montoApertura = 0;
            montoFisicoEfectivoReal = 0;
            montoFisicoTarjetaReal = 0;
            montoFisicoTransferenciaReal = 0;
            statusMessage = result.Data?.AsientoContableId is null
                ? "La caja se cerro correctamente."
                : "La caja se cerro correctamente y el asiento contable fue generado.";
            await PopupNotificationService.ShowSuccessAsync(statusMessage);
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cerrar la caja.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
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
