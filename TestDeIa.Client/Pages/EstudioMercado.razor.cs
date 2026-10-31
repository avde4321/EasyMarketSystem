using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Compras;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Client.Pages;

public partial class EstudioMercado
{
    [Inject]
    private EstudioMercadoApiClient EstudioMercadoApiClient { get; set; } = default!;

    private readonly IReadOnlyCollection<(int Numero, string Nombre)> meses =
    [
        (1, "Enero"), (2, "Febrero"), (3, "Marzo"), (4, "Abril"),
        (5, "Mayo"), (6, "Junio"), (7, "Julio"), (8, "Agosto"),
        (9, "Septiembre"), (10, "Octubre"), (11, "Noviembre"), (12, "Diciembre")
    ];

    private readonly IReadOnlyCollection<int> anios =
        Enumerable.Range(DateTime.Now.Year - 5, 7).Reverse().ToArray();

    private EstudioMercadoCompraResponse estudio = new();
    private IReadOnlyCollection<string> analysisParagraphs = Array.Empty<string>();
    private int selectedMes = DateTime.Now.Month;
    private int selectedAnio = DateTime.Now.Year;
    private bool isLoading;
    private bool isLoaded;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await ConsultarAsync();
    }

    private async Task ConsultarAsync()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            estudio = await EstudioMercadoApiClient.GetAsync(selectedMes, selectedAnio);
            analysisParagraphs = estudio.AnalisisEstrategicoIA
                .Split([Environment.NewLine + Environment.NewLine], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            isLoaded = true;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo generar el estudio de mercado de la empresa activa.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private IReadOnlyCollection<PodiumItemViewModel> BuildPodium()
    {
        var max = estudio.TopProductosVendidos.Count == 0
            ? 1m
            : Math.Max(1m, estudio.TopProductosVendidos.Max(current => current.CantidadVendida));

        return estudio.TopProductosVendidos.Select(current => new PodiumItemViewModel
        {
            Codigo = current.Codigo,
            Nombre = current.Nombre,
            CantidadVendida = current.CantidadVendida,
            MargenEstimado = current.MargenEstimado,
            StockActual = current.StockActual,
            Width = $"{Math.Max(16m, Math.Round((current.CantidadVendida / max) * 100m, 2, MidpointRounding.AwayFromZero))}%"
        }).ToArray();
    }

    private sealed class PodiumItemViewModel
    {
        public string Codigo { get; init; } = string.Empty;
        public string Nombre { get; init; } = string.Empty;
        public decimal CantidadVendida { get; init; }
        public decimal MargenEstimado { get; init; }
        public decimal StockActual { get; init; }
        public string Width { get; init; } = "0%";
    }
}
