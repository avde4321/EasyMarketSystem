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
    private readonly List<TestDeIa.Shared.Responses.Empresa.EmpresaResponse> empresas = [];
    private readonly List<CatalogoItemResponse> ambientesSri = [];
    private readonly List<CatalogoItemResponse> tiposEmision = [];
    private bool isSaving;
    private string? errorMessage;
    private string? statusMessage;
    private string? certificadoNombreArchivoActual;
    private Guid? selectedEmpresaId;
    private string searchTerm = string.Empty;

    private IEnumerable<TestDeIa.Shared.Responses.Empresa.EmpresaResponse> FilteredEmpresas => empresas.Where(empresa =>
        string.IsNullOrWhiteSpace(searchTerm) ||
        empresa.Ruc.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        empresa.RazonSocial.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        (empresa.NombreComercial?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
        empresa.AmbienteSri.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        empresa.Establecimiento.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        empresa.PuntoEmision.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

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
            empresas.Clear();
            var opciones = await EmpresaApiClient.GetMineAsync();

            foreach (var opcion in opciones)
            {
                var detalle = await EmpresaApiClient.GetByIdAsync(opcion.Id);
                if (detalle is not null)
                {
                    empresas.Add(detalle);
                }
            }

            if (selectedEmpresaId.HasValue)
            {
                await SelectEmpresaAsync(selectedEmpresaId.Value);
                return;
            }

            if (empresas.Count == 0)
            {
                StartCreate();
                return;
            }

            await SelectEmpresaAsync(empresas[0].Id);
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
            var result = selectedEmpresaId.HasValue
                ? await EmpresaApiClient.UpdateAsync(selectedEmpresaId.Value, empresaRequest)
                : await EmpresaApiClient.CreateAsync(empresaRequest);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            selectedEmpresaId = result.Data?.Id;
            statusMessage = selectedEmpresaId.HasValue
                ? "La empresa fue guardada correctamente."
                : "La empresa fue creada correctamente.";
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

    private async Task SelectEmpresaAsync(Guid id)
    {
        selectedEmpresaId = id;
        empresaActual = empresas.FirstOrDefault(current => current.Id == id) ?? await EmpresaApiClient.GetByIdAsync(id);

        if (empresaActual is null)
        {
            errorMessage = "No se pudo cargar la empresa seleccionada.";
            return;
        }

        empresaRequest = new EmpresaRequest
        {
            RazonSocial = empresaActual.RazonSocial,
            NombreComercial = empresaActual.NombreComercial,
            Ruc = empresaActual.Ruc,
            DireccionMatriz = empresaActual.DireccionMatriz,
            DireccionEstablecimiento = empresaActual.DireccionEstablecimiento,
            Establecimiento = empresaActual.Establecimiento,
            PuntoEmision = empresaActual.PuntoEmision,
            AmbienteSri = empresaActual.AmbienteSri,
            ModoDesarrollo = empresaActual.ModoDesarrollo,
            TipoEmision = empresaActual.TipoEmision,
            ObligadoContabilidad = empresaActual.ObligadoContabilidad,
            ContribuyenteEspecial = empresaActual.ContribuyenteEspecial,
            RegimenRimpe = empresaActual.RegimenRimpe,
            AgenteRetencionResolucion = empresaActual.AgenteRetencionResolucion,
            CertificadoNombreArchivo = empresaActual.CertificadoNombreArchivo,
            IsActive = empresaActual.IsActive
        };

        certificadoNombreArchivoActual = empresaActual.CertificadoNombreArchivo;
        StateHasChanged();
    }

    private void StartCreate()
    {
        selectedEmpresaId = null;
        empresaActual = null;
        certificadoNombreArchivoActual = null;
        empresaRequest = new EmpresaRequest
        {
            AmbienteSri = ambientesSri.FirstOrDefault()?.Codigo ?? "Pruebas",
            TipoEmision = tiposEmision.FirstOrDefault()?.Codigo ?? "Normal",
            Establecimiento = "001",
            PuntoEmision = "001",
            IsActive = true,
            ModoDesarrollo = true
        };
    }
}
