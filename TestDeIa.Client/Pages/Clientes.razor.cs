using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Clientes;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Clientes;
using TestDeIa.Shared.Responses.Personas;

namespace TestDeIa.Client.Pages;

public partial class Clientes
{
    [Inject]
    private ClientesApiClient ClientesApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    [Inject]
    private PersonasApiClient PersonasApiClient { get; set; } = default!;

    private readonly List<ClienteResponse> clientes = [];
    private readonly List<CatalogoItemResponse> tiposIdentificacion = [];
    private readonly List<PersonaResponse> personasCoincidentes = [];
    private ClienteRequest clienteRequest = new();
    private Guid? editingClienteId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isEditorOpen;
    private bool isSearchingPersona;
    private bool isSearchingPersonas;
    private bool showClienteRoleForm;
    private bool isClienteFromExistingPersona;
    private string? errorMessage;
    private string? statusMessage;
    private string searchTerm = string.Empty;
    private string personaSearchTerm = string.Empty;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;
    private IEnumerable<ClienteResponse> VisibleClientes => clientes;
    private bool HasPersonaSearchTerm => !string.IsNullOrWhiteSpace(personaSearchTerm);
    private bool ShowPersonaBaseFields => editingClienteId.HasValue || !isClienteFromExistingPersona;
    private static bool TieneRolCliente(PersonaResponse persona) => persona.RolesPersona.Contains("Cliente", StringComparer.OrdinalIgnoreCase);
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
        await LoadClientesAsync(resetPaging: true);
    }

    private async Task LoadCatalogosAsync()
    {
        tiposIdentificacion.Clear();
        tiposIdentificacion.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_IDENTIFICACION", true));
    }

    private async Task LoadClientesAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var page = await ClientesApiClient.GetPagedAsync(searchTerm, currentSkip, PageSize);
            clientes.Clear();
            clientes.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar la lista de clientes.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task OpenCreateModalAsync()
    {
        editingClienteId = null;
        clienteRequest = new ClienteRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "05",
            TipoCliente = "Natural",
            EstadoCredito = "AlDia"
        };
        errorMessage = null;
        statusMessage = null;
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
        showClienteRoleForm = false;
        isClienteFromExistingPersona = false;
        isEditorOpen = true;
        await BuscarPersonasCoincidentesAsync();
    }

    private void OpenEditModal(ClienteResponse cliente)
    {
        editingClienteId = cliente.Id;
        errorMessage = null;
        clienteRequest = new ClienteRequest
        {
            TipoIdentificacion = cliente.TipoIdentificacion,
            Identificacion = cliente.Identificacion,
            RazonSocialONombresCompletos = cliente.RazonSocialONombresCompletos,
            NombreComercial = cliente.NombreComercial,
            CorreoElectronicoPrincipal = cliente.CorreoElectronicoPrincipal,
            TelefonoCelular = cliente.TelefonoCelular,
            DireccionPrincipal = cliente.DireccionPrincipal,
            RegionCodigo = cliente.RegionCodigo,
            ProvinciaCodigo = cliente.ProvinciaCodigo,
            CiudadCodigo = cliente.CiudadCodigo,
            SectorCodigo = cliente.SectorCodigo,
            CorreoFacturacionElectronica = cliente.CorreoFacturacionElectronica,
            TipoCliente = cliente.TipoCliente,
            ObligadoContabilidad = cliente.ObligadoContabilidad,
            EsContribuyenteEspecial = cliente.EsContribuyenteEspecial,
            PermiteCredito = cliente.PermiteCredito,
            LimiteCredito = cliente.LimiteCredito,
            DiasCreditoMaximo = cliente.DiasCreditoMaximo,
            EstadoCredito = cliente.EstadoCredito,
            FechaNacimiento = cliente.FechaNacimiento,
            Genero = cliente.Genero,
            IsActive = cliente.IsActive
        };
        isEditorOpen = true;
        showClienteRoleForm = true;
        isClienteFromExistingPersona = false;
        statusMessage = null;
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
    }

    private void CloseModal()
    {
        isEditorOpen = false;
        isSaving = false;
        showClienteRoleForm = false;
        isClienteFromExistingPersona = false;
        errorMessage = null;
        statusMessage = null;
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
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
                ? "No hay coincidencias. Puedes usar Nuevo registro para ingresar un cliente desde cero."
                : "Selecciona una persona existente para autollenar el formulario y evitar duplicados.";
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
        clienteRequest.TipoIdentificacion = persona.TipoIdentificacion;
        clienteRequest.Identificacion = persona.Identificacion;
        clienteRequest.RazonSocialONombresCompletos = persona.RazonSocialONombresCompletos;
        clienteRequest.NombreComercial = persona.NombreComercial;
        clienteRequest.CorreoElectronicoPrincipal = persona.CorreoElectronicoPrincipal;
        clienteRequest.TelefonoCelular = persona.TelefonoCelular;
        clienteRequest.DireccionPrincipal = persona.DireccionPrincipal;
        clienteRequest.RegionCodigo = persona.RegionCodigo;
        clienteRequest.ProvinciaCodigo = persona.ProvinciaCodigo;
        clienteRequest.CiudadCodigo = persona.CiudadCodigo;
        clienteRequest.SectorCodigo = persona.SectorCodigo;
        clienteRequest.FechaNacimiento = persona.FechaNacimiento;
        clienteRequest.Genero = persona.Genero;
        clienteRequest.IsActive = persona.IsActive;
        showClienteRoleForm = !TieneRolCliente(persona);
        isClienteFromExistingPersona = showClienteRoleForm;

        if (string.IsNullOrWhiteSpace(clienteRequest.CorreoFacturacionElectronica))
        {
            clienteRequest.CorreoFacturacionElectronica = persona.CorreoElectronicoPrincipal;
        }

        statusMessage = TieneRolCliente(persona)
            ? "Esta persona ya figura como Cliente. Si necesitas cambiar datos, usa Editar desde la consulta principal."
            : "Datos autollenados. Completa la informacion comercial del cliente y guarda.";
    }

    private void NuevoRegistroCliente()
    {
        editingClienteId = null;
        clienteRequest = new ClienteRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "05",
            TipoCliente = "Natural",
            EstadoCredito = "AlDia",
            IsActive = true
        };
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
        errorMessage = null;
        showClienteRoleForm = true;
        isClienteFromExistingPersona = false;
        statusMessage = "Formulario limpio para registrar un cliente nuevo.";
    }

    private void VolverAConsultaPersonasCliente()
    {
        showClienteRoleForm = false;
        isClienteFromExistingPersona = false;
        statusMessage = "Selecciona una persona existente o crea un nuevo registro.";
    }

    private async Task BuscarPersonaAsync()
    {
        errorMessage = null;
        statusMessage = null;

        if (string.IsNullOrWhiteSpace(clienteRequest.Identificacion))
        {
            errorMessage = "Ingresa una identificacion antes de buscar.";
            return;
        }

        isSearchingPersona = true;

        try
        {
            var persona = await PersonasApiClient.FindByIdentificacionAsync(clienteRequest.Identificacion);
            if (persona is null)
            {
                statusMessage = "No se encontro una persona registrada con esa identificacion.";
                return;
            }

            clienteRequest.TipoIdentificacion = persona.TipoIdentificacion;
            clienteRequest.Identificacion = persona.Identificacion;
            clienteRequest.RazonSocialONombresCompletos = persona.RazonSocialONombresCompletos;
            clienteRequest.NombreComercial = persona.NombreComercial;
            clienteRequest.CorreoElectronicoPrincipal = persona.CorreoElectronicoPrincipal;
            clienteRequest.TelefonoCelular = persona.TelefonoCelular;
            clienteRequest.DireccionPrincipal = persona.DireccionPrincipal;
            clienteRequest.RegionCodigo = persona.RegionCodigo;
            clienteRequest.ProvinciaCodigo = persona.ProvinciaCodigo;
            clienteRequest.CiudadCodigo = persona.CiudadCodigo;
            clienteRequest.SectorCodigo = persona.SectorCodigo;
            clienteRequest.FechaNacimiento = persona.FechaNacimiento;
            clienteRequest.Genero = persona.Genero;
            clienteRequest.IsActive = persona.IsActive;
            if (string.IsNullOrWhiteSpace(clienteRequest.CorreoFacturacionElectronica))
            {
                clienteRequest.CorreoFacturacionElectronica = persona.CorreoElectronicoPrincipal;
            }

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

    private async Task SaveClienteAsync()
    {
        isSaving = true;
        errorMessage = null;

        try
        {
            var result = editingClienteId.HasValue
                ? await ClientesApiClient.UpdateAsync(editingClienteId.Value, clienteRequest)
                : await ClientesApiClient.CreateAsync(clienteRequest);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseModal();
            await LoadClientesAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar el cliente.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DeleteClienteAsync(Guid id)
    {
        errorMessage = null;

        try
        {
            await ClientesApiClient.DeleteAsync(id);
            await LoadClientesAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo eliminar el cliente.";
        }
    }

    private async Task SearchAsync()
    {
        await LoadClientesAsync(resetPaging: true);
    }

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadClientesAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadClientesAsync();
    }
}
