using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Compras;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Client.Pages;

public partial class Proveedores
{
    [Inject]
    private ProveedoresApiClient ProveedoresApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    [Inject]
    private PersonasApiClient PersonasApiClient { get; set; } = default!;

    private readonly List<ProveedorResponse> proveedores = [];
    private readonly List<CatalogoItemResponse> tiposIdentificacion = [];
    private ProveedorRequest proveedorRequest = new();
    private Guid? editingProveedorId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isEditorOpen;
    private bool isSearchingPersona;
    private string? errorMessage;
    private string? statusMessage;
    private string searchTerm = string.Empty;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;
    private IEnumerable<ProveedorResponse> VisibleProveedores => proveedores;
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
        await LoadProveedoresAsync(resetPaging: true);
    }

    private async Task LoadCatalogosAsync()
    {
        tiposIdentificacion.Clear();
        tiposIdentificacion.AddRange((await CatalogosApiClient.GetItemsAsync("TIPO_IDENTIFICACION", true))
            .Where(item => item.Codigo is "04" or "06"));
    }

    private async Task LoadProveedoresAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var page = await ProveedoresApiClient.GetPagedAsync(searchTerm, currentSkip, PageSize);
            proveedores.Clear();
            proveedores.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar la lista de proveedores.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenCreateModal()
    {
        editingProveedorId = null;
        proveedorRequest = new ProveedorRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "04",
            EstadoProveedor = "Activo",
            CodigoRetencionIvaDefault = "0",
            CodigoRetencionRentaDefault = "0"
        };
        errorMessage = null;
        statusMessage = null;
        isEditorOpen = true;
    }

    private void OpenEditModal(ProveedorResponse proveedor)
    {
        editingProveedorId = proveedor.Id;
        errorMessage = null;
        proveedorRequest = new ProveedorRequest
        {
            TipoIdentificacion = proveedor.TipoIdentificacion,
            Identificacion = proveedor.Identificacion,
            RazonSocialONombresCompletos = proveedor.RazonSocialONombresCompletos,
            NombreComercial = proveedor.NombreComercial,
            CorreoElectronicoPrincipal = proveedor.CorreoElectronicoPrincipal,
            TelefonoCelular = proveedor.TelefonoCelular,
            DireccionPrincipal = proveedor.DireccionPrincipal,
            FechaNacimiento = proveedor.FechaNacimiento,
            Genero = proveedor.Genero,
            CodigoRetencionIvaDefault = proveedor.CodigoRetencionIvaDefault,
            CodigoRetencionRentaDefault = proveedor.CodigoRetencionRentaDefault,
            PermiteCredito = proveedor.PermiteCredito,
            DiasCredito = proveedor.DiasCredito,
            EstadoProveedor = proveedor.EstadoProveedor,
            IsActive = proveedor.IsActive
        };
        isEditorOpen = true;
        statusMessage = null;
    }

    private void CloseModal()
    {
        isEditorOpen = false;
        isSaving = false;
        errorMessage = null;
        statusMessage = null;
    }

    private async Task BuscarPersonaAsync()
    {
        errorMessage = null;
        statusMessage = null;

        if (string.IsNullOrWhiteSpace(proveedorRequest.Identificacion))
        {
            errorMessage = "Ingresa una identificacion antes de buscar.";
            return;
        }

        isSearchingPersona = true;

        try
        {
            var persona = await PersonasApiClient.FindByIdentificacionAsync(proveedorRequest.Identificacion);
            if (persona is null)
            {
                statusMessage = "No se encontro una persona registrada con esa identificacion.";
                return;
            }

            proveedorRequest.TipoIdentificacion = persona.TipoIdentificacion;
            proveedorRequest.Identificacion = persona.Identificacion;
            proveedorRequest.RazonSocialONombresCompletos = persona.RazonSocialONombresCompletos;
            proveedorRequest.NombreComercial = persona.NombreComercial;
            proveedorRequest.CorreoElectronicoPrincipal = persona.CorreoElectronicoPrincipal;
            proveedorRequest.TelefonoCelular = persona.TelefonoCelular;
            proveedorRequest.DireccionPrincipal = persona.DireccionPrincipal;
            proveedorRequest.FechaNacimiento = persona.FechaNacimiento;
            proveedorRequest.Genero = persona.Genero;
            proveedorRequest.IsActive = persona.IsActive;

            statusMessage = "Se cargo la informacion de la persona existente.";
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo consultar la persona.";
        }
        finally
        {
            isSearchingPersona = false;
        }
    }

    private async Task SaveProveedorAsync()
    {
        isSaving = true;
        errorMessage = null;

        try
        {
            var result = editingProveedorId.HasValue
                ? await ProveedoresApiClient.UpdateAsync(editingProveedorId.Value, proveedorRequest)
                : await ProveedoresApiClient.CreateAsync(proveedorRequest);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseModal();
            await LoadProveedoresAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar el proveedor.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DeleteProveedorAsync(Guid id)
    {
        errorMessage = null;

        try
        {
            await ProveedoresApiClient.DeleteAsync(id);
            await LoadProveedoresAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo eliminar el proveedor.";
        }
    }

    private async Task SearchAsync()
    {
        await LoadProveedoresAsync(resetPaging: true);
    }

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadProveedoresAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadProveedoresAsync();
    }
}
