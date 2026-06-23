using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Clientes;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Responses.Clientes;

namespace TestDeIa.Client.Pages;

public partial class Clientes
{
    [Inject]
    private ClientesApiClient ClientesApiClient { get; set; } = default!;

    private readonly List<ClienteResponse> clientes = [];
    private ClienteRequest clienteRequest = new();
    private Guid? editingClienteId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isEditorOpen;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadClientesAsync();
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
        clienteRequest = new ClienteRequest();
        errorMessage = null;
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
    }

    private void CloseModal()
    {
        isEditorOpen = false;
        isSaving = false;
        errorMessage = null;
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
