using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Clientes;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Clientes;

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
    private ClienteRequest clienteRequest = new();
    private Guid? editingClienteId;
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
    private IEnumerable<ClienteResponse> VisibleClientes => clientes;
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

    private void OpenCreateModal()
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
        isEditorOpen = true;
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
            clienteRequest.FechaNacimiento = persona.FechaNacimiento;
            clienteRequest.Genero = persona.Genero;
            clienteRequest.IsActive = persona.IsActive;

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
