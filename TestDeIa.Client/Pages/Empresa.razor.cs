using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Empresa;
using TestDeIa.Client.Services.Inventario;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Inventario;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Client.Pages;

public partial class Empresa
{
    [Inject]
    private EmpresaApiClient EmpresaApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    [Inject]
    private InventarioApiClient InventarioApiClient { get; set; } = default!;

    private static readonly string[] WorkflowSteps = ["Datos generales", "Certificado", "Puntos de emisión"];

    private EmpresaRequest empresaRequest = new();
    private TestDeIa.Shared.Responses.Empresa.EmpresaResponse? empresaActual;
    private readonly List<TestDeIa.Shared.Responses.Empresa.EmpresaResponse> empresas = [];
    private readonly List<CatalogoItemResponse> ambientesSri = [];
    private readonly List<CatalogoItemResponse> tiposEmision = [];
    private readonly List<BodegaResponse> bodegas = [];
    private bool isSaving;
    private bool showWorkflowModal;
    private bool showPuntosEmisionModal;
    private int currentStep = 1;
    private string? errorMessage;
    private string? statusMessage;
    private string? certificadoNombreArchivoActual;
    private Guid? selectedEmpresaId;
    private string searchTerm = string.Empty;
    private EmpresaPuntoEmisionRequest puntoEmisionDraft = new() { Establecimiento = "001", PuntoEmision = "001" };
    private int? editingPuntoIndex;
    private const int PageSize = 8;
    private int totalCount;
    private int currentSkip;
    private IEnumerable<TestDeIa.Shared.Responses.Empresa.EmpresaResponse> VisibleEmpresas => empresas;
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
    private IReadOnlyList<BodegaResponse> activeBodegas => bodegas.Where(current => current.IsActive).OrderBy(current => current.Nombre).ToList();
    private int CurrentStep => currentStep;

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
        await LoadBodegasAsync();
        await LoadAsync(resetPaging: true);
    }

    private async Task LoadCatalogosAsync()
    {
        ambientesSri.Clear();
        tiposEmision.Clear();
        ambientesSri.AddRange(await CatalogosApiClient.GetItemsAsync("AMBIENTE_SRI", true));
        tiposEmision.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_EMISION", true));
    }

    private async Task LoadBodegasAsync()
    {
        bodegas.Clear();
        bodegas.AddRange(await InventarioApiClient.GetBodegasAsync());
    }

    private async Task LoadAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        errorMessage = null;

        try
        {
            empresas.Clear();
            var page = await EmpresaApiClient.GetPagedAsync(searchTerm, currentSkip, PageSize);
            empresas.AddRange(page.Items);
            totalCount = page.TotalCount;

            if (empresas.Count == 0)
            {
                StartCreate();
                return;
            }

            var targetEmpresaId = selectedEmpresaId.HasValue && empresas.Any(current => current.Id == selectedEmpresaId.Value)
                ? selectedEmpresaId.Value
                : empresas[0].Id;

            await LoadEmpresaIntoEditorAsync(targetEmpresaId, openWorkflow: false);
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar la configuración de la empresa.";
        }
    }

    private async Task SaveAsync()
    {
        isSaving = true;
        errorMessage = null;
        statusMessage = null;
        var isUpdate = selectedEmpresaId.HasValue;

        try
        {
            var result = isUpdate && selectedEmpresaId.HasValue
                ? await EmpresaApiClient.UpdateAsync(selectedEmpresaId.Value, empresaRequest)
                : await EmpresaApiClient.CreateAsync(empresaRequest);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            selectedEmpresaId = result.Data?.Id;
            statusMessage = isUpdate
                ? "La empresa fue actualizada correctamente."
                : "La empresa fue creada correctamente.";
            showWorkflowModal = false;
            showPuntosEmisionModal = false;
            currentStep = 1;
            await LoadAsync(resetPaging: true);
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar la configuración de la empresa.";
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
            errorMessage = "Selecciona un archivo .p12 válido.";
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
        await LoadEmpresaIntoEditorAsync(id, openWorkflow: true);
    }

    private async Task LoadEmpresaIntoEditorAsync(Guid id, bool openWorkflow)
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
            RegionCodigo = empresaActual.RegionCodigo,
            ProvinciaCodigo = empresaActual.ProvinciaCodigo,
            CiudadCodigo = empresaActual.CiudadCodigo,
            SectorCodigo = empresaActual.SectorCodigo,
            AmbienteSri = SriCatalogCodes.NormalizeAmbienteCode(empresaActual.AmbienteSri) ?? empresaActual.AmbienteSri,
            ModoDesarrollo = empresaActual.ModoDesarrollo,
            TipoEmision = SriCatalogCodes.NormalizeTipoEmisionCode(empresaActual.TipoEmision) ?? empresaActual.TipoEmision,
            ObligadoContabilidad = empresaActual.ObligadoContabilidad,
            ContribuyenteEspecial = empresaActual.ContribuyenteEspecial,
            RegimenRimpe = empresaActual.RegimenRimpe,
            AgenteRetencionResolucion = empresaActual.AgenteRetencionResolucion,
            CertificadoNombreArchivo = empresaActual.CertificadoNombreArchivo,
            IsActive = empresaActual.IsActive,
            PuntosEmision = empresaActual.PuntosEmision
                .Select(punto => new EmpresaPuntoEmisionRequest
                {
                    Id = punto.Id,
                    BodegaId = punto.BodegaId,
                    DireccionEstablecimiento = punto.DireccionEstablecimiento,
                    Establecimiento = punto.Establecimiento,
                    PuntoEmision = punto.PuntoEmision,
                    IsDefault = punto.IsDefault
                })
                .ToList()
        };

        certificadoNombreArchivoActual = empresaActual.CertificadoNombreArchivo;
        currentStep = 1;
        showPuntosEmisionModal = false;
        if (openWorkflow)
        {
            showWorkflowModal = true;
        }
        StateHasChanged();
    }

    private void StartCreate()
    {
        selectedEmpresaId = null;
        empresaActual = null;
        certificadoNombreArchivoActual = null;
        empresaRequest = new EmpresaRequest
        {
            AmbienteSri = ambientesSri.FirstOrDefault()?.Codigo ?? "1",
            TipoEmision = tiposEmision.FirstOrDefault()?.Codigo ?? "1",
            IsActive = true,
            ModoDesarrollo = true,
            PuntosEmision =
            [
                new EmpresaPuntoEmisionRequest
                {
                    Establecimiento = "001",
                    PuntoEmision = "001",
                    BodegaId = activeBodegas.FirstOrDefault()?.Id,
                    IsDefault = true
                }
            ]
        };

        ResetPuntoEmisionDraft();
        currentStep = 1;
        showPuntosEmisionModal = false;
        showWorkflowModal = true;
    }

    private void AddPuntoEmision()
    {
        if (editingPuntoIndex.HasValue)
        {
            empresaRequest.PuntosEmision[editingPuntoIndex.Value] = new EmpresaPuntoEmisionRequest
            {
                Id = puntoEmisionDraft.Id,
                BodegaId = puntoEmisionDraft.BodegaId,
                DireccionEstablecimiento = puntoEmisionDraft.DireccionEstablecimiento,
                Establecimiento = puntoEmisionDraft.Establecimiento,
                PuntoEmision = puntoEmisionDraft.PuntoEmision,
                IsDefault = puntoEmisionDraft.IsDefault
            };
        }
        else
        {
            empresaRequest.PuntosEmision.Add(new EmpresaPuntoEmisionRequest
            {
                Id = puntoEmisionDraft.Id,
                BodegaId = puntoEmisionDraft.BodegaId,
                DireccionEstablecimiento = puntoEmisionDraft.DireccionEstablecimiento,
                Establecimiento = puntoEmisionDraft.Establecimiento,
                PuntoEmision = puntoEmisionDraft.PuntoEmision,
                IsDefault = puntoEmisionDraft.IsDefault || empresaRequest.PuntosEmision.Count == 0
            });
        }

        if (puntoEmisionDraft.IsDefault || empresaRequest.PuntosEmision.Count == 1)
        {
            var targetIndex = editingPuntoIndex ?? (empresaRequest.PuntosEmision.Count - 1);
            SetDefaultPuntoEmision(targetIndex);
        }

        ResetPuntoEmisionDraft();
        showPuntosEmisionModal = false;
    }

    private void RemovePuntoEmision(int index)
    {
        if (empresaRequest.PuntosEmision.Count <= 1)
        {
            errorMessage = "La empresa debe conservar al menos un punto de emisión.";
            return;
        }

        var wasDefault = empresaRequest.PuntosEmision[index].IsDefault;
        empresaRequest.PuntosEmision.RemoveAt(index);

        if (wasDefault && empresaRequest.PuntosEmision.Count > 0)
        {
            empresaRequest.PuntosEmision[0].IsDefault = true;
        }
    }

    private void SetDefaultPuntoEmision(int index)
    {
        for (var currentIndex = 0; currentIndex < empresaRequest.PuntosEmision.Count; currentIndex++)
        {
            empresaRequest.PuntosEmision[currentIndex].IsDefault = currentIndex == index;
        }
    }

    private void EditPuntoEmision(int index)
    {
        var punto = empresaRequest.PuntosEmision[index];
        editingPuntoIndex = index;
        puntoEmisionDraft = new EmpresaPuntoEmisionRequest
        {
            Id = punto.Id,
            BodegaId = punto.BodegaId,
            DireccionEstablecimiento = punto.DireccionEstablecimiento,
            Establecimiento = punto.Establecimiento,
            PuntoEmision = punto.PuntoEmision,
            IsDefault = punto.IsDefault
        };

        showPuntosEmisionModal = true;
    }

    private void ResetPuntoEmisionDraft()
    {
        editingPuntoIndex = null;
        puntoEmisionDraft = new EmpresaPuntoEmisionRequest
        {
            Establecimiento = "001",
            PuntoEmision = "001",
            BodegaId = activeBodegas.FirstOrDefault()?.Id,
            IsDefault = empresaRequest.PuntosEmision.Count == 0
        };
    }

    private string GetBodegaName(Guid? bodegaId)
    {
        if (!bodegaId.HasValue || bodegaId.Value == Guid.Empty)
        {
            return "Se asignará Principal";
        }

        return activeBodegas.FirstOrDefault(current => current.Id == bodegaId.Value)?.Nombre ?? "Bodega no disponible";
    }

    private string GetPuntosResumenDraft()
    {
        if (empresaRequest.PuntosEmision.Count == 0)
        {
            return "Todavía no hay puntos configurados.";
        }

        var principal = empresaRequest.PuntosEmision.FirstOrDefault(punto => punto.IsDefault) ?? empresaRequest.PuntosEmision.First();
        return empresaRequest.PuntosEmision.Count == 1
            ? $"Predeterminado: {principal.Establecimiento}-{principal.PuntoEmision}"
            : $"Predeterminado: {principal.Establecimiento}-{principal.PuntoEmision} + {empresaRequest.PuntosEmision.Count - 1} adicional(es)";
    }

    private void OpenWorkflowModal()
    {
        if (!selectedEmpresaId.HasValue && empresas.Count == 0)
        {
            StartCreate();
            return;
        }

        showWorkflowModal = true;
        errorMessage = null;
        statusMessage = null;
    }

    private void CloseWorkflowModal()
    {
        showWorkflowModal = false;
        showPuntosEmisionModal = false;
    }

    private void OpenPuntosEmisionModal()
    {
        ResetPuntoEmisionDraft();
        showPuntosEmisionModal = true;
    }

    private void ClosePuntosEmisionModal()
    {
        showPuntosEmisionModal = false;
        ResetPuntoEmisionDraft();
    }

    private void GoToStep(int step)
    {
        if (step >= 1 && step <= WorkflowSteps.Length)
        {
            currentStep = step;
        }
    }

    private void GoPreviousStep()
    {
        if (currentStep > 1)
        {
            currentStep--;
        }
    }

    private void GoNextStep()
    {
        if (currentStep < WorkflowSteps.Length)
        {
            currentStep++;
        }
    }

    private static string GetPuntosResumen(TestDeIa.Shared.Responses.Empresa.EmpresaResponse empresa)
    {
        if (empresa.PuntosEmision.Count == 0)
        {
            return $"{empresa.Establecimiento}-{empresa.PuntoEmision}";
        }

        var principal = empresa.PuntosEmision.FirstOrDefault(punto => punto.IsDefault) ?? empresa.PuntosEmision.First();
        return empresa.PuntosEmision.Count == 1
            ? $"{principal.Establecimiento}-{principal.PuntoEmision}"
            : $"{principal.Establecimiento}-{principal.PuntoEmision} + {empresa.PuntosEmision.Count - 1}";
    }

    private static string GetAmbienteLabel(string? value) => SriCatalogCodes.GetAmbienteName(value);

    private static string GetTipoEmisionLabel(string? value) => SriCatalogCodes.GetTipoEmisionName(value);

    private Task SearchAsync() => LoadAsync(resetPaging: true);

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadAsync();
    }
}
