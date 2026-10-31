using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Compras;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Compras;
using TestDeIa.Shared.Responses.Personas;

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
    private readonly List<PersonaResponse> personasCoincidentes = [];
    private ProveedorRequest proveedorRequest = new();
    private Guid? editingProveedorId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isEditorOpen;
    private bool isSearchingPersona;
    private bool isSearchingPersonas;
    private bool showProveedorRoleForm;
    private bool isProveedorFromExistingPersona;
    private string? errorMessage;
    private string? statusMessage;
    private string searchTerm = string.Empty;
    private string personaSearchTerm = string.Empty;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;
    private IEnumerable<ProveedorResponse> VisibleProveedores => proveedores;
    private bool HasPersonaSearchTerm => !string.IsNullOrWhiteSpace(personaSearchTerm);
    private bool ShowPersonaBaseFields => editingProveedorId.HasValue || !isProveedorFromExistingPersona;
    private static bool TieneRolProveedor(PersonaResponse persona) => persona.RolesPersona.Contains("Proveedor", StringComparer.OrdinalIgnoreCase);
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

    private async Task OpenCreateModalAsync()
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
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
        showProveedorRoleForm = false;
        isProveedorFromExistingPersona = false;
        isEditorOpen = true;
        await BuscarPersonasCoincidentesAsync();
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
            RegionCodigo = proveedor.RegionCodigo,
            ProvinciaCodigo = proveedor.ProvinciaCodigo,
            CiudadCodigo = proveedor.CiudadCodigo,
            SectorCodigo = proveedor.SectorCodigo,
            CodigoRetencionIvaDefault = proveedor.CodigoRetencionIvaDefault,
            CodigoRetencionRentaDefault = proveedor.CodigoRetencionRentaDefault,
            PermiteCredito = proveedor.PermiteCredito,
            DiasCredito = proveedor.DiasCredito,
            EstadoProveedor = proveedor.EstadoProveedor,
            IsActive = proveedor.IsActive
        };
        isEditorOpen = true;
        showProveedorRoleForm = true;
        isProveedorFromExistingPersona = false;
        statusMessage = null;
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
    }

    private void CloseModal()
    {
        isEditorOpen = false;
        isSaving = false;
        errorMessage = null;
        statusMessage = null;
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
        showProveedorRoleForm = false;
        isProveedorFromExistingPersona = false;
    }

    private async Task OnPersonaSearchChangedAsync(ChangeEventArgs args)
    {
        personaSearchTerm = args.Value?.ToString() ?? string.Empty;

        if (personaSearchTerm.Trim().Length < 2)
        {
            personaSearchTerm = string.Empty;
        }

        await BuscarPersonasCoincidentesAsync();
    }

    private async Task BuscarPersonasCoincidentesAsync()
    {
        isSearchingPersonas = true;
        errorMessage = null;

        try
        {
            var page = await PersonasApiClient.GetPagedAsync(personaSearchTerm, 0, 12);
            personasCoincidentes.Clear();
            personasCoincidentes.AddRange(page.Items);
            statusMessage = personasCoincidentes.Count == 0
                ? "No hay coincidencias. Puedes usar Nuevo registro para crear la persona y proveedor desde cero."
                : "Convierte una persona existente en proveedor sin duplicar su identidad.";
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo consultar personas existentes.";
        }
        finally
        {
            isSearchingPersonas = false;
        }
    }

    private void SeleccionarPersona(PersonaResponse persona)
    {
        proveedorRequest.TipoIdentificacion = persona.TipoIdentificacion;
        proveedorRequest.Identificacion = persona.Identificacion;
        proveedorRequest.RazonSocialONombresCompletos = persona.RazonSocialONombresCompletos;
        proveedorRequest.NombreComercial = persona.NombreComercial;
        proveedorRequest.CorreoElectronicoPrincipal = persona.CorreoElectronicoPrincipal;
        proveedorRequest.TelefonoCelular = persona.TelefonoCelular;
        proveedorRequest.DireccionPrincipal = persona.DireccionPrincipal;
        proveedorRequest.RegionCodigo = persona.RegionCodigo;
        proveedorRequest.ProvinciaCodigo = persona.ProvinciaCodigo;
        proveedorRequest.CiudadCodigo = persona.CiudadCodigo;
        proveedorRequest.SectorCodigo = persona.SectorCodigo;
        proveedorRequest.IsActive = persona.IsActive;
        showProveedorRoleForm = !TieneRolProveedor(persona);
        isProveedorFromExistingPersona = showProveedorRoleForm;

        statusMessage = TieneRolProveedor(persona)
            ? "Esta persona ya figura como Proveedor. Si necesitas cambiar datos, usa Editar desde la consulta principal."
            : "Datos base cargados por debajo. Completa solamente la informacion comercial de compras.";
    }

    private void NuevoRegistroProveedor()
    {
        editingProveedorId = null;
        proveedorRequest = new ProveedorRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "04",
            EstadoProveedor = "Activo",
            CodigoRetencionIvaDefault = "0",
            CodigoRetencionRentaDefault = "0",
            IsActive = true
        };
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
        errorMessage = null;
        showProveedorRoleForm = true;
        isProveedorFromExistingPersona = false;
        statusMessage = "Formulario limpio para registrar una persona nueva como proveedor.";
    }

    private void VolverAConsultaPersonasProveedor()
    {
        showProveedorRoleForm = false;
        isProveedorFromExistingPersona = false;
        statusMessage = "Selecciona una persona existente o crea un nuevo registro.";
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
            proveedorRequest.RegionCodigo = persona.RegionCodigo;
            proveedorRequest.ProvinciaCodigo = persona.ProvinciaCodigo;
            proveedorRequest.CiudadCodigo = persona.CiudadCodigo;
            proveedorRequest.SectorCodigo = persona.SectorCodigo;
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
