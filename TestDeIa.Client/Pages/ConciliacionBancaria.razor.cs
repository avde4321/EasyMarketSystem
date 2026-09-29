using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Tesoreria;
using TestDeIa.Shared.Requests.Tesoreria;
using TestDeIa.Shared.Responses.Tesoreria;

namespace TestDeIa.Client.Pages;

public partial class ConciliacionBancaria
{
    [Parameter] public Guid CuentaId { get; set; }
    [Inject] private TesoreriaApiClient TesoreriaApiClient { get; set; } = default!;

    private IReadOnlyCollection<MovimientoTesoreriaDto> movimientos = Array.Empty<MovimientoTesoreriaDto>();
    private IReadOnlyCollection<ExtractoBancarioDetalleDto> extractos = Array.Empty<ExtractoBancarioDetalleDto>();
    private IReadOnlyCollection<ConciliacionMatchDto> suggestions = Array.Empty<ConciliacionMatchDto>();
    private MovimientoTesoreriaDto? selectedMovimiento;
    private ExtractoBancarioDetalleDto? selectedExtracto;
    private DateTime? fechaDesde = DateTime.Today.AddDays(-30);
    private DateTime? fechaHasta = DateTime.Today;
    private string? tipoMovimiento;
    private string? searchTerm;
    private string? errorMessage;
    private string? successMessage;
    private bool isBusy;

    private decimal SelectedMovimientoTotal => selectedMovimiento?.Monto ?? 0m;
    private decimal SelectedExtractoTotal => selectedExtracto?.Monto ?? 0m;
    private bool CanConciliarSeleccion => selectedMovimiento is not null && selectedExtracto is not null;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        errorMessage = null;
        successMessage = null;
        byte? tipo = byte.TryParse(tipoMovimiento, out var parsed) ? parsed : null;
        movimientos = await TesoreriaApiClient.GetMovimientosPendientesAsync(CuentaId, fechaDesde, fechaHasta, searchTerm);
        extractos = await TesoreriaApiClient.GetExtractosPendientesAsync(CuentaId, fechaDesde, fechaHasta, tipo, searchTerm);
        selectedMovimiento = null;
        selectedExtracto = null;
    }

    private async Task RunAutoMatchAsync()
    {
        isBusy = true;
        errorMessage = null;
        var result = await TesoreriaApiClient.ConciliarAutomaticoAsync(CuentaId);
        isBusy = false;

        if (!result.Succeeded || result.Data is null)
        {
            errorMessage = result.ErrorMessage;
            return;
        }

        suggestions = result.Data.Coincidencias;
        successMessage = $"Conciliación ejecutada: {result.Data.TotalConciliados} conciliados y {result.Data.TotalSugerencias} sugerencias.";
        await LoadAsync();
        suggestions = result.Data.Coincidencias;
    }

    private void SelectMovimiento(MovimientoTesoreriaDto item)
    {
        selectedMovimiento = selectedMovimiento?.Id == item.Id ? null : item;
    }

    private void SelectExtracto(ExtractoBancarioDetalleDto item)
    {
        selectedExtracto = selectedExtracto?.Id == item.Id ? null : item;
    }

    private async Task ConciliarSeleccionAsync()
    {
        if (!CanConciliarSeleccion || selectedMovimiento is null || selectedExtracto is null)
        {
            return;
        }

        isBusy = true;
        var result = await TesoreriaApiClient.ConciliarManualAsync(new ConciliacionManualDto
        {
            MovimientoTesoreriaId = selectedMovimiento.Id,
            ExtractoDetalleId = selectedExtracto.Id,
            EsAjusteAutomatico = false
        });
        isBusy = false;

        if (!result.Succeeded)
        {
            errorMessage = result.ErrorMessage;
            return;
        }

        successMessage = "Selección conciliada correctamente.";
        await LoadAsync();
    }

    private Task CrearNotaDebitoAsync()
    {
        errorMessage = "La creación automática de nota de débito/comisión requiere definir la cuenta contable de gasto bancario. El botón queda reservado para la siguiente fase.";
        return Task.CompletedTask;
    }

    private ConciliacionMatchDto? GetSuggestion(Guid extractoId)
    {
        return suggestions.FirstOrDefault(current => current.ExtractoDetalleId == extractoId);
    }

    private static string ResolveSuggestionClass(ConciliacionMatchDto? suggestion)
    {
        if (suggestion is null)
        {
            return string.Empty;
        }

        return suggestion.Confianza >= 100 ? "match-exact" : "match-suggested";
    }
}
