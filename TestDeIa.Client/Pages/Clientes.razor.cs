using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Clientes;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Clientes;
using TestDeIa.Shared.Responses.Personas;
using TestDeIa.Shared.Clientes;

namespace TestDeIa.Client.Pages;

public partial class Clientes
{
    [Inject]
    private TestDeIa.Client.Services.PopupNotificationService PopupNotificationService { get; set; } = default!;

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
    private static readonly string[] clientFormTabs = ["Identificación", "Contacto", "Facturación y crédito"];
    private int activeClientTab;
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
    private bool EsClienteJuridico => clienteRequest.TipoCliente == TipoClienteCodes.Juridico;
    private bool EsConsumidorFinal => clienteRequest.TipoIdentificacion == "07";
    private bool MostrarDatosPersonales => !EsClienteJuridico && !EsConsumidorFinal;
    private bool MostrarDatosTributarios => clienteRequest.TipoIdentificacion == "04";
    private bool MostrarNombreComercial => EsClienteJuridico || clienteRequest.TipoIdentificacion == "04";
    private void OnTipoClienteChanged()
    {
        if (EsClienteJuridico)
        {
            clienteRequest.TipoIdentificacion = "04";
            clienteRequest.FechaNacimiento = null;
            clienteRequest.Genero = null;
        }
        OnTipoIdentificacionChanged();
    }

    private void OnTipoIdentificacionChanged()
    {
        if (EsConsumidorFinal)
        {
            clienteRequest.TipoCliente = TipoClienteCodes.Natural;
            clienteRequest.Identificacion = "9999999999999";
            clienteRequest.RazonSocialONombresCompletos = "CONSUMIDOR FINAL";
            clienteRequest.PermiteCredito = false;
            clienteRequest.LimiteCredito = 0;
            clienteRequest.DiasCreditoMaximo = 0;
            clienteRequest.EstadoCredito = EstadoCreditoClienteCodes.AlDia;
        }
        else if (clienteRequest.Identificacion == "9999999999999")
        {
            clienteRequest.Identificacion = string.Empty;
            if (clienteRequest.RazonSocialONombresCompletos == "CONSUMIDOR FINAL")
                clienteRequest.RazonSocialONombresCompletos = string.Empty;
        }
        if (!MostrarNombreComercial) clienteRequest.NombreComercial = null;
        if (!MostrarDatosPersonales) { clienteRequest.FechaNacimiento = null; clienteRequest.Genero = null; }
        if (!MostrarDatosTributarios) { clienteRequest.ObligadoContabilidad = false; clienteRequest.EsContribuyenteEspecial = false; }
    }
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
            TipoIdentificacion = "05",
            TipoCliente = "Natural",
            EstadoCredito = "AlDia"
        };
        activeClientTab = 0;
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
            EstadoCredito = EstadoCreditoClienteCodes.Normalize(cliente.EstadoCredito) ?? cliente.EstadoCredito,
            FechaNacimiento = cliente.FechaNacimiento,
            Genero = cliente.Genero,
            IsActive = cliente.IsActive
        };
        activeClientTab = 0;
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
        clienteRequest = new ClienteRequest();
        activeClientTab = 1;
        clienteRequest.TipoIdentificacion = persona.TipoIdentificacion;
        clienteRequest.TipoCliente = persona.EsPersonaJuridica ? TipoClienteCodes.Juridico : TipoClienteCodes.Natural;
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
            TipoIdentificacion = "05",
            TipoCliente = "Natural",
            EstadoCredito = "AlDia",
            IsActive = true
        };
        activeClientTab = 0;
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

    private void OnInvalidClienteSubmit(EditContext context)
    {
        var identityFields = new[] { "TipoCliente", "TipoIdentificacion", "Identificacion", "RazonSocialONombresCompletos" };
        var contactFields = new[] { "NombreComercial", "CorreoElectronicoPrincipal", "TelefonoCelular", "FechaNacimiento", "Genero", "DireccionPrincipal" };
        activeClientTab = identityFields.Any(field => context.GetValidationMessages(new FieldIdentifier(clienteRequest, field)).Any())
            ? 0
            : contactFields.Any(field => context.GetValidationMessages(new FieldIdentifier(clienteRequest, field)).Any())
                ? 1
                : 2;
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

            SeleccionarPersona(persona);
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
        if (isSaving) return;
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
            await PopupNotificationService.ShowSuccessAsync("Cliente guardado correctamente.");
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
