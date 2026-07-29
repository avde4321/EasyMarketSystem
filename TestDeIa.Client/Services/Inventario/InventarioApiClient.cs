using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Inventario;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Client.Services.Inventario;

public sealed class InventarioApiClient
{
    private readonly HttpClient httpClient;

    public InventarioApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<BodegaResponse>> GetBodegasAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<BodegaResponse>>("api/inventario/bodegas")
            ?? Array.Empty<BodegaResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateBodegaAsync(BodegaRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/inventario/bodegas", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateBodegaAsync(Guid id, BodegaRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/inventario/bodegas/{id}", request);
        return await BuildResultAsync(response);
    }

    public async Task<PagedResultResponse<ProductoResponse>> GetProductosAsync(string? term, int skip, int take, Guid? bodegaId = null)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        var bodegaQuery = bodegaId.HasValue && bodegaId.Value != Guid.Empty ? $"&bodegaId={bodegaId.Value}" : string.Empty;
        return await httpClient.GetFromJsonAsync<PagedResultResponse<ProductoResponse>>($"api/inventario/productos?term={encodedTerm}&skip={skip}&take={take}{bodegaQuery}")
            ?? new PagedResultResponse<ProductoResponse> { Skip = skip, Take = take };
    }

    public async Task<IReadOnlyCollection<KardexMovimientoResponse>> GetKardexAsync(Guid productoId, Guid? bodegaId = null)
    {
        var bodegaQuery = bodegaId.HasValue && bodegaId.Value != Guid.Empty ? $"?bodegaId={bodegaId.Value}" : string.Empty;
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<KardexMovimientoResponse>>(
            $"api/inventario/productos/{productoId}/kardex{bodegaQuery}") ?? Array.Empty<KardexMovimientoResponse>();
    }

    public async Task<IReadOnlyCollection<StockDisponibleBodegaResponse>> GetDisponibilidadEnOtrasBodegasAsync(Guid productoId, Guid? bodegaActualId = null)
    {
        var bodegaQuery = bodegaActualId.HasValue && bodegaActualId.Value != Guid.Empty ? $"?bodegaActualId={bodegaActualId.Value}" : string.Empty;
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<StockDisponibleBodegaResponse>>(
            $"api/inventario/productos/{productoId}/disponibilidad-bodegas{bodegaQuery}") ?? Array.Empty<StockDisponibleBodegaResponse>();
    }

    public async Task<IReadOnlyCollection<StockAlertaResponse>> GetAlertasStockAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<StockAlertaResponse>>("api/inventario/alertas-stock")
            ?? Array.Empty<StockAlertaResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateProductoAsync(ProductoRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/inventario/productos", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateProductoAsync(Guid id, ProductoRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/inventario/productos/{id}", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> AjustarStockAsync(Guid productoId, AjusteStockRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"api/inventario/productos/{productoId}/ajustes", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> RegistrarMermaAsync(EgresoMermaRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/inventario/movimientos/merma", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> TransferirStockAsync(TransferenciaInventarioRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/inventario/movimientos/transferencia", request);
        return await BuildResultAsync(response);
    }

    public async Task<IReadOnlyCollection<TransferenciaInventarioResponse>> GetTransferenciasAsync(string? estado = null, Guid? bodegaOrigenId = null, Guid? bodegaDestinoId = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(estado))
        {
            query.Add($"estado={Uri.EscapeDataString(estado)}");
        }

        if (bodegaOrigenId.HasValue && bodegaOrigenId.Value != Guid.Empty)
        {
            query.Add($"bodegaOrigenId={bodegaOrigenId.Value}");
        }

        if (bodegaDestinoId.HasValue && bodegaDestinoId.Value != Guid.Empty)
        {
            query.Add($"bodegaDestinoId={bodegaDestinoId.Value}");
        }

        var queryString = query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}";
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<TransferenciaInventarioResponse>>($"api/inventario/transferencias{queryString}")
            ?? Array.Empty<TransferenciaInventarioResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateTransferenciaAsync(TransferenciaInventarioFormalRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/inventario/transferencias", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> DespacharTransferenciaAsync(Guid id)
    {
        var response = await httpClient.PostAsync($"api/inventario/transferencias/{id}/despachar", null);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> RecibirTransferenciaAsync(Guid id, RecepcionTransferenciaInventarioRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"api/inventario/transferencias/{id}/recibir", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage, TomaFisicaResultadoResponse? Data)> ProcesarTomaFisicaAsync(TomaFisicaInventarioRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/inventario/tomas-fisicas", request);

        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<TomaFisicaResultadoResponse>());
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo procesar la toma fisica.", null);
        }

        return (false, "No se pudo procesar la toma fisica.", null);
    }

    private static async Task<(bool Succeeded, string? ErrorMessage)> BuildResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo completar la operacion.");
        }

        return (false, "No se pudo completar la operacion.");
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
