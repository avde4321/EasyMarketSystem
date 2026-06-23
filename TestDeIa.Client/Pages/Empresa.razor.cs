using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Empresa;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Catalogos;

namespace TestDeIa.Client.Pages;

public partial class Empresa
{
    [Inject]
    private EmpresaApiClient EmpresaApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    private EmpresaRequest empresaRequest = new();
    private TestDeIa.Shared.Responses.Empresa.EmpresaResponse? empresaActual;
    private readonly List<CatalogoItemResponse> ambientesSri = [];
    private readonly List<CatalogoItemResponse> tiposEmision = [];
    private bool isSaving;
    private string? errorMessage;
    private string? statusMessage;
    private string? certificadoNombreArchivoActual;

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
        await LoadAsync();
    }

    private async Task LoadCatalogosAsync()
    {
        ambientesSri.Clear();
        tiposEmision.Clear();
        ambientesSri.AddRange(await CatalogosApiClient.GetItemsAsync("AMBIENTE_SRI", true));
        tiposEmision.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_EMISION", true));
    }

    private async Task LoadAsync()
    {
        errorMessage = null;

        try
        {
            var empresa = await EmpresaApiClient.GetCurrentAsync();
            if (empresa is null)
            {
                empresaActual = null;
                empresaRequest = new EmpresaRequest
                {
                    AmbienteSri = ambientesSri.FirstOrDefault()?.Codigo ?? "Pruebas",
                    TipoEmision = tiposEmision.FirstOrDefault()?.Codigo ?? "Normal"
                };
                return;
            }

            empresaActual = empresa;
            empresaRequest = new EmpresaRequest
            {
                RazonSocial = empresa.RazonSocial,
                NombreComercial = empresa.NombreComercial,
                Ruc = empresa.Ruc,
                DireccionMatriz = empresa.DireccionMatriz,
                DireccionEstablecimiento = empresa.DireccionEstablecimiento,
                Establecimiento = empresa.Establecimiento,
                PuntoEmision = empresa.PuntoEmision,
                AmbienteSri = empresa.AmbienteSri,
                ModoDesarrollo = empresa.ModoDesarrollo,
                TipoEmision = empresa.TipoEmision,
                ObligadoContabilidad = empresa.ObligadoContabilidad,
                ContribuyenteEspecial = empresa.ContribuyenteEspecial,
                RegimenRimpe = empresa.RegimenRimpe,
                AgenteRetencionResolucion = empresa.AgenteRetencionResolucion,
                CertificadoNombreArchivo = empresa.CertificadoNombreArchivo,
                IsActive = empresa.IsActive
            };
            certificadoNombreArchivoActual = empresa.CertificadoNombreArchivo;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar la configuracion de la empresa.";
        }
    }

    private async Task SaveAsync()
    {
        isSaving = true;
        errorMessage = null;
        statusMessage = null;

        try
        {
            var result = await EmpresaApiClient.SaveAsync(empresaRequest);
            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            statusMessage = "La configuracion tributaria de la empresa fue actualizada.";
            await LoadAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar la configuracion de la empresa.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task OnCertificadoSelectedAsync(InputFileChangeEventArgs eventArgs)
    {
        errorMessage = null;
        statusMessage = null;

        var file = eventArgs.File;
        if (file is null)
        {
            return;
        }

        if (!file.Name.EndsWith(".p12", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Selecciona un archivo .p12 valido.";
            return;
        }

        await using var stream = file.OpenReadStream(5 * 1024 * 1024);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        empresaRequest.CertificadoNombreArchivo = file.Name;
        empresaRequest.CertificadoContenido = memoryStream.ToArray();
        certificadoNombreArchivoActual = file.Name;
    }
}
