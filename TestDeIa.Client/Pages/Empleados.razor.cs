using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Empleados;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Shared.Requests.Empleados;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Empleados;
using TestDeIa.Shared.Responses.Personas;

namespace TestDeIa.Client.Pages;

public partial class Empleados
{
    [Inject]
    private EmpleadosApiClient EmpleadosApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    [Inject]
    private PersonasApiClient PersonasApiClient { get; set; } = default!;

    private readonly List<EmpleadoResponse> empleados = [];
    private readonly List<CatalogoItemResponse> tiposIdentificacion = [];
    private readonly List<PersonaResponse> personasCoincidentes = [];
    private EmpleadoRequest empleadoRequest = new();
    private Guid? editingEmpleadoId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isEditorOpen;
    private bool isSearchingPersona;
    private bool isSearchingPersonas;
    private bool showEmpleadoRoleForm;
    private bool isEmpleadoFromExistingPersona;
    private string? errorMessage;
    private string? statusMessage;
    private string searchTerm = string.Empty;
    private string personaSearchTerm = string.Empty;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;
    private IEnumerable<EmpleadoResponse> VisibleEmpleados => empleados;
    private bool HasPersonaSearchTerm => !string.IsNullOrWhiteSpace(personaSearchTerm);
    private bool ShowPersonaBaseFields => editingEmpleadoId.HasValue || !isEmpleadoFromExistingPersona;
    private static bool TieneRolEmpleado(PersonaResponse persona) => persona.RolesPersona.Contains("Empleado", StringComparer.OrdinalIgnoreCase);
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
        await LoadEmpleadosAsync(resetPaging: true);
    }

    private async Task LoadCatalogosAsync()
    {
        tiposIdentificacion.Clear();
        tiposIdentificacion.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_IDENTIFICACION", true));
    }

    private async Task LoadEmpleadosAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var page = await EmpleadosApiClient.GetPagedAsync(searchTerm, currentSkip, PageSize);
            empleados.Clear();
            empleados.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar la lista de empleados.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task OpenCreateModalAsync()
    {
        editingEmpleadoId = null;
        empleadoRequest = new EmpleadoRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "05",
            TipoContrato = "Indefinido",
            EstadoLaboral = "Activo"
        };
        errorMessage = null;
        statusMessage = null;
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
        showEmpleadoRoleForm = false;
        isEmpleadoFromExistingPersona = false;
        isEditorOpen = true;
        await BuscarPersonasCoincidentesAsync();
    }

    private void OpenEditModal(EmpleadoResponse empleado)
    {
        editingEmpleadoId = empleado.Id;
        empleadoRequest = new EmpleadoRequest
        {
            TipoIdentificacion = empleado.TipoIdentificacion,
            Identificacion = empleado.Identificacion,
            RazonSocialONombresCompletos = empleado.RazonSocialONombresCompletos,
            CorreoElectronicoPrincipal = empleado.CorreoElectronicoPrincipal,
            TelefonoCelular = empleado.TelefonoCelular,
            DireccionPrincipal = empleado.DireccionPrincipal,
            RegionCodigo = empleado.RegionCodigo,
            ProvinciaCodigo = empleado.ProvinciaCodigo,
            CiudadCodigo = empleado.CiudadCodigo,
            SectorCodigo = empleado.SectorCodigo,
            FechaNacimiento = empleado.FechaNacimiento,
            Genero = empleado.Genero,
            CodigoEmpleado = empleado.CodigoEmpleado,
            CodigoBiometrico = empleado.CodigoBiometrico,
            FechaIngreso = empleado.FechaIngreso,
            FechaSalida = empleado.FechaSalida,
            TipoContrato = empleado.TipoContrato,
            CargoPuesto = empleado.CargoPuesto,
            SueldoBase = empleado.SueldoBase,
            PorcentajeComisionVentas = empleado.PorcentajeComisionVentas,
            EstadoLaboral = empleado.EstadoLaboral,
            NombreContactoEmergencia = empleado.NombreContactoEmergencia,
            TelefonoEmergencia = empleado.TelefonoEmergencia,
            IsActive = empleado.IsActive
        };
        errorMessage = null;
        statusMessage = null;
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
        showEmpleadoRoleForm = true;
        isEmpleadoFromExistingPersona = false;
        isEditorOpen = true;
    }

    private void CloseModal()
    {
        isEditorOpen = false;
        isSaving = false;
        errorMessage = null;
        statusMessage = null;
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
        showEmpleadoRoleForm = false;
        isEmpleadoFromExistingPersona = false;
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
                ? "No hay coincidencias. Puedes usar Nuevo registro para crear la persona y empleado desde cero."
                : "Convierte una persona existente en empleado sin duplicar su identidad.";
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
        if (persona.EsEmpresa)
        {
            showEmpleadoRoleForm = false;
            isEmpleadoFromExistingPersona = false;
            statusMessage = "Las empresas no pueden convertirse en empleados. Selecciona una persona natural.";
            return;
        }

        empleadoRequest.TipoIdentificacion = persona.TipoIdentificacion;
        empleadoRequest.Identificacion = persona.Identificacion;
        empleadoRequest.RazonSocialONombresCompletos = persona.RazonSocialONombresCompletos;
        empleadoRequest.NombreComercial = persona.NombreComercial;
        empleadoRequest.CorreoElectronicoPrincipal = persona.CorreoElectronicoPrincipal;
        empleadoRequest.TelefonoCelular = persona.TelefonoCelular;
        empleadoRequest.DireccionPrincipal = persona.DireccionPrincipal;
        empleadoRequest.RegionCodigo = persona.RegionCodigo;
        empleadoRequest.ProvinciaCodigo = persona.ProvinciaCodigo;
        empleadoRequest.CiudadCodigo = persona.CiudadCodigo;
        empleadoRequest.SectorCodigo = persona.SectorCodigo;
        empleadoRequest.FechaNacimiento = persona.FechaNacimiento;
        empleadoRequest.Genero = persona.Genero;
        empleadoRequest.IsActive = persona.IsActive;
        showEmpleadoRoleForm = !TieneRolEmpleado(persona);
        isEmpleadoFromExistingPersona = showEmpleadoRoleForm;

        statusMessage = TieneRolEmpleado(persona)
            ? "Esta persona ya figura como Empleado. Si necesitas cambiar datos, usa Editar desde la consulta principal."
            : "Datos autollenados. Completa la informacion laboral del empleado y guarda.";
    }

    private void NuevoRegistroEmpleado()
    {
        editingEmpleadoId = null;
        empleadoRequest = new EmpleadoRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "05",
            TipoContrato = "Indefinido",
            EstadoLaboral = "Activo",
            IsActive = true
        };
        personaSearchTerm = string.Empty;
        personasCoincidentes.Clear();
        errorMessage = null;
        showEmpleadoRoleForm = true;
        isEmpleadoFromExistingPersona = false;
        statusMessage = "Formulario limpio para registrar una persona nueva como empleado.";
    }

    private void VolverAConsultaPersonasEmpleado()
    {
        showEmpleadoRoleForm = false;
        isEmpleadoFromExistingPersona = false;
        statusMessage = "Selecciona una persona existente o crea un nuevo registro.";
    }

    private async Task BuscarPersonaAsync()
    {
        errorMessage = null;
        statusMessage = null;

        if (string.IsNullOrWhiteSpace(empleadoRequest.Identificacion))
        {
            errorMessage = "Ingresa una identificacion antes de buscar.";
            return;
        }

        isSearchingPersona = true;

        try
        {
            var persona = await PersonasApiClient.FindByIdentificacionAsync(empleadoRequest.Identificacion);
            if (persona is null)
            {
                statusMessage = "No se encontro una persona registrada con esa identificacion.";
                return;
            }

            if (persona.EsEmpresa)
            {
                statusMessage = "Las empresas no pueden convertirse en empleados. Selecciona una persona natural.";
                return;
            }

            empleadoRequest.TipoIdentificacion = persona.TipoIdentificacion;
            empleadoRequest.Identificacion = persona.Identificacion;
            empleadoRequest.RazonSocialONombresCompletos = persona.RazonSocialONombresCompletos;
            empleadoRequest.CorreoElectronicoPrincipal = persona.CorreoElectronicoPrincipal;
            empleadoRequest.TelefonoCelular = persona.TelefonoCelular;
            empleadoRequest.DireccionPrincipal = persona.DireccionPrincipal;
            empleadoRequest.RegionCodigo = persona.RegionCodigo;
            empleadoRequest.ProvinciaCodigo = persona.ProvinciaCodigo;
            empleadoRequest.CiudadCodigo = persona.CiudadCodigo;
            empleadoRequest.SectorCodigo = persona.SectorCodigo;
            empleadoRequest.FechaNacimiento = persona.FechaNacimiento;
            empleadoRequest.Genero = persona.Genero;
            empleadoRequest.IsActive = persona.IsActive;

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

    private async Task SaveEmpleadoAsync()
    {
        isSaving = true;
        errorMessage = null;
        empleadoRequest.NombreComercial = null;

        try
        {
            var result = editingEmpleadoId.HasValue
                ? await EmpleadosApiClient.UpdateAsync(editingEmpleadoId.Value, empleadoRequest)
                : await EmpleadosApiClient.CreateAsync(empleadoRequest);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseModal();
            await LoadEmpleadosAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar el empleado.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DeleteEmpleadoAsync(Guid id)
    {
        errorMessage = null;

        try
        {
            await EmpleadosApiClient.DeleteAsync(id);
            await LoadEmpleadosAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo eliminar el empleado.";
        }
    }

    private async Task SearchAsync()
    {
        await LoadEmpleadosAsync(resetPaging: true);
    }

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadEmpleadosAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadEmpleadosAsync();
    }
}
