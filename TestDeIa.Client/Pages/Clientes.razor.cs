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

    private IEnumerable<ClienteResponse> FilteredClientes => clientes.Where(cliente =>
        string.IsNullOrWhiteSpace(searchTerm) ||
        cliente.TipoIdentificacion.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        cliente.Identificacion.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        cliente.NombreCompleto.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        (cliente.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (cliente.Telefono?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (cliente.Direccion?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
        cliente.RolesPersona.Any(role => role.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
        await LoadClientesAsync();
    }

    private async Task LoadCatalogosAsync()
    {
        tiposIdentificacion.Clear();
        tiposIdentificacion.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_IDENTIFICACION", true));
    }

    private async Task LoadClientesAsync()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            clientes.Clear();
            clientes.AddRange(await ClientesApiClient.GetAllAsync());
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
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "Cedula"
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
            Nombres = cliente.Nombres,
            Apellidos = cliente.Apellidos,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            Direccion = cliente.Direccion,
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
            clienteRequest.Nombres = persona.Nombres;
            clienteRequest.Apellidos = persona.Apellidos;
            clienteRequest.Email = persona.Email;
            clienteRequest.Telefono = persona.Telefono;
            clienteRequest.Direccion = persona.Direccion;
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
}
