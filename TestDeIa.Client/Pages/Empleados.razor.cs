using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Empleados;
using TestDeIa.Shared.Requests.Empleados;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Empleados;

namespace TestDeIa.Client.Pages;

public partial class Empleados
{
    [Inject]
    private EmpleadosApiClient EmpleadosApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    private readonly List<EmpleadoResponse> empleados = [];
    private readonly List<CatalogoItemResponse> tiposIdentificacion = [];
    private EmpleadoRequest empleadoRequest = new();
    private Guid? editingEmpleadoId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isEditorOpen;
    private string? errorMessage;
    private string searchTerm = string.Empty;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;
    private IEnumerable<EmpleadoResponse> VisibleEmpleados => empleados;
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

    private void OpenCreateModal()
    {
        editingEmpleadoId = null;
        empleadoRequest = new EmpleadoRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "Cedula"
        };
        errorMessage = null;
        isEditorOpen = true;
    }

    private void OpenEditModal(EmpleadoResponse empleado)
    {
        editingEmpleadoId = empleado.Id;
        empleadoRequest = new EmpleadoRequest
        {
            TipoIdentificacion = empleado.TipoIdentificacion,
            Identificacion = empleado.Identificacion,
            Nombres = empleado.Nombres,
            Apellidos = empleado.Apellidos,
            Email = empleado.Email,
            Telefono = empleado.Telefono,
            Direccion = empleado.Direccion,
            IsActive = empleado.IsActive
        };
        errorMessage = null;
        isEditorOpen = true;
    }

    private void CloseModal()
    {
        isEditorOpen = false;
        isSaving = false;
        errorMessage = null;
    }

    private async Task SaveEmpleadoAsync()
    {
        isSaving = true;
        errorMessage = null;

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
