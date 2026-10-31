using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Tesoreria;
using TestDeIa.Shared.Responses.Tesoreria;

namespace TestDeIa.Client.Services.Tesoreria;

public sealed class TesoreriaApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<CuentaBancariaDto>> GetCuentasBancariasAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<CuentaBancariaDto>>("api/tesoreria/cuentas-bancarias")
            ?? Array.Empty<CuentaBancariaDto>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage, CuentaBancariaDto? Data)> SaveCuentaBancariaAsync(Guid? id, CuentaBancariaRequest request)
    {
        var url = id.HasValue && id.Value != Guid.Empty
            ? $"api/tesoreria/cuentas-bancarias?id={id.Value}"
            : "api/tesoreria/cuentas-bancarias";

        var response = await httpClient.PostAsJsonAsync(url, request);
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<CuentaBancariaDto>());
        }

        return (false, await ReadErrorAsync(response, "No se pudo guardar la cuenta bancaria."), null);
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ExtractoBancarioHeaderDto? Data)> ImportarExtractoAsync(Guid cuentaBancariaId, Stream fileStream, string fileName, string? formato)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(cuentaBancariaId.ToString()), "cuentaBancariaId");
        if (!string.IsNullOrWhiteSpace(formato))
        {
            content.Add(new StringContent(formato), "formato");
        }

        content.Add(new StreamContent(fileStream), "archivo", fileName);
        var response = await httpClient.PostAsync("api/tesoreria/importar-extracto", content);
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<ExtractoBancarioHeaderDto>());
        }

        return (false, await ReadErrorAsync(response, "No se pudo importar el extracto bancario."), null);
    }

    public async Task<IReadOnlyCollection<MovimientoTesoreriaDto>> GetMovimientosPendientesAsync(Guid cuentaId, DateTime? desde, DateTime? hasta, string? term)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<MovimientoTesoreriaDto>>(
            $"api/tesoreria/movimientos-pendientes/{cuentaId}{BuildQuery(desde, hasta, null, term)}")
            ?? Array.Empty<MovimientoTesoreriaDto>();
    }

    public async Task<IReadOnlyCollection<ExtractoBancarioDetalleDto>> GetExtractosPendientesAsync(Guid cuentaId, DateTime? desde, DateTime? hasta, byte? tipoMovimiento, string? term)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<ExtractoBancarioDetalleDto>>(
            $"api/tesoreria/extractos-pendientes/{cuentaId}{BuildQuery(desde, hasta, tipoMovimiento, term)}")
            ?? Array.Empty<ExtractoBancarioDetalleDto>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ConciliacionResultadoDto? Data)> ConciliarAutomaticoAsync(Guid cuentaId)
    {
        var response = await httpClient.PostAsync($"api/tesoreria/conciliar-automatico/{cuentaId}", null);
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<ConciliacionResultadoDto>());
        }

        return (false, await ReadErrorAsync(response, "No se pudo ejecutar la conciliación automática."), null);
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ConciliacionMatchDto? Data)> ConciliarManualAsync(ConciliacionManualDto request)
    {
        var response = await httpClient.PostAsJsonAsync("api/tesoreria/conciliar-manual", request);
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<ConciliacionMatchDto>());
        }

        return (false, await ReadErrorAsync(response, "No se pudo conciliar la selección."), null);
    }

    private static string BuildQuery(DateTime? desde, DateTime? hasta, byte? tipoMovimiento, string? term)
    {
        var values = new List<string>();
        if (desde.HasValue)
        {
            values.Add($"desde={desde.Value:yyyy-MM-dd}");
        }

        if (hasta.HasValue)
        {
            values.Add($"hasta={hasta.Value:yyyy-MM-dd}");
        }

        if (tipoMovimiento.HasValue)
        {
            values.Add($"tipoMovimiento={tipoMovimiento.Value}");
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            values.Add($"term={Uri.EscapeDataString(term)}");
        }

        return values.Count == 0 ? string.Empty : $"?{string.Join("&", values)}";
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, string fallback)
    {
        if (response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return error?.Message ?? fallback;
        }

        return fallback;
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
