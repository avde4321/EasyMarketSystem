using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using TestDeIa.Client.Services.Financiero;
using TestDeIa.Shared.Responses.Financiero;

namespace TestDeIa.Client.Pages;

public partial class ReportesIva
{
    [Inject]
    private FinancieroApiClient FinancieroApiClient { get; set; } = default!;

    private readonly IReadOnlyCollection<(int Numero, string Nombre)> meses =
    [
        (1, "Enero"), (2, "Febrero"), (3, "Marzo"), (4, "Abril"),
        (5, "Mayo"), (6, "Junio"), (7, "Julio"), (8, "Agosto"),
        (9, "Septiembre"), (10, "Octubre"), (11, "Noviembre"), (12, "Diciembre")
    ];

    private readonly IReadOnlyCollection<int> anios =
        Enumerable.Range(DateTime.Now.Year - 5, 7).Reverse().ToArray();

    private ConsolidadoIvaMensualResponse consolidado = new();
    private IReadOnlyCollection<string> analysisParagraphs = Array.Empty<string>();
    private int selectedMes = DateTime.Now.Month;
    private int selectedAnio = DateTime.Now.Year;
    private string puntoEmisionFiltro = string.Empty;
    private string cajeroFiltro = string.Empty;
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
            consolidado = await FinancieroApiClient.GetConsolidadoIvaAsync(selectedMes, selectedAnio, puntoEmisionFiltro, cajeroFiltro);
            analysisParagraphs = consolidado.RazonamientoIA
                .Split([Environment.NewLine + Environment.NewLine], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            isLoaded = true;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo consultar el consolidado mensual de IVA.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private RenderFragment RenderTarifaRow(string label, decimal baseImponible, decimal iva) => builder =>
    {
        RenderBaseRow(builder, label, baseImponible, iva, false);
    };

    private RenderFragment RenderHighlightRow(string label, decimal baseImponible, decimal iva) => builder =>
    {
        RenderBaseRow(builder, label, baseImponible, iva, true);
    };

    private static void RenderBaseRow(RenderTreeBuilder builder, string label, decimal baseImponible, decimal iva, bool highlight)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", highlight ? "iva-row iva-row-highlight" : "iva-row");

        builder.OpenElement(2, "span");
        builder.AddAttribute(3, "class", "iva-row-label");
        builder.AddContent(4, label);
        builder.CloseElement();

        builder.OpenElement(5, "span");
        builder.AddAttribute(6, "class", "iva-row-value");
        builder.AddContent(7, baseImponible.ToString("C2"));
        builder.CloseElement();

        builder.OpenElement(8, "span");
        builder.AddAttribute(9, "class", "iva-row-value");
        builder.AddContent(10, iva.ToString("C2"));
        builder.CloseElement();

        builder.CloseElement();
    }
}
