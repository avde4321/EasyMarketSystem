using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Security;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Client.Pages;

public partial class Usuarios
{
    [Inject]
    private SecurityApiClient SecurityApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    [Inject]
    private PersonasApiClient PersonasApiClient { get; set; } = default!;

    private readonly List<SecurityUserResponse> users = [];
    private readonly List<SecurityRoleResponse> roles = [];
    private readonly List<CatalogoItemResponse> tiposIdentificacion = [];
    private readonly HashSet<string> selectedRoles = new(StringComparer.OrdinalIgnoreCase);
    private SecurityUserRequest userRequest = new();
    private Guid? editingUserId;
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
    private IEnumerable<SecurityUserResponse> VisibleUsers => users;
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        await LoadLookupsAsync();
        await LoadUsersAsync(resetPaging: true);
    }

    private async Task LoadLookupsAsync()
    {
        roles.Clear();
        tiposIdentificacion.Clear();
        roles.AddRange(await SecurityApiClient.GetRolesAsync());
        tiposIdentificacion.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_IDENTIFICACION", true));
    }

    private async Task LoadUsersAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var page = await SecurityApiClient.GetUsersAsync(searchTerm, currentSkip, PageSize);
            users.Clear();
            users.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar la lista de usuarios.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenCreateModal()
    {
        editingUserId = null;
        userRequest = new SecurityUserRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "05",
            IsActive = true
        };
        selectedRoles.Clear();
        errorMessage = null;
        statusMessage = null;
        isEditorOpen = true;
    }

    private void OpenEditModal(SecurityUserResponse user)
    {
        editingUserId = user.Id;

        var names = user.PersonaNombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var nombres = names.Length > 0 ? names[0] : user.PersonaNombre;
        var apellidos = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        userRequest = new SecurityUserRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "05",
            Identificacion = user.PersonaIdentificacion,
            Nombres = nombres,
            Apellidos = apellidos,
            Email = user.Email,
            UserName = user.UserName,
            IsActive = user.IsActive,
            Roles = user.Roles.ToArray()
        };

        selectedRoles.Clear();
        foreach (var role in user.Roles)
        {
            selectedRoles.Add(role);
        }

        errorMessage = null;
        statusMessage = null;
        isEditorOpen = true;
    }

    private void ToggleRole(string roleName, object? value)
    {
        var isChecked = value as bool? == true;

        if (isChecked)
        {
            selectedRoles.Add(roleName);
        }
        else
        {
            selectedRoles.Remove(roleName);
        }
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

        if (string.IsNullOrWhiteSpace(userRequest.Identificacion))
        {
            errorMessage = "Ingresa una identificacion antes de buscar.";
            return;
        }

        isSearchingPersona = true;

        try
        {
            var persona = await PersonasApiClient.FindByIdentificacionAsync(userRequest.Identificacion);
            if (persona is null)
            {
                statusMessage = "No se encontro una persona registrada con esa identificacion.";
                return;
            }

            userRequest.TipoIdentificacion = persona.TipoIdentificacion;
            userRequest.Identificacion = persona.Identificacion;
            userRequest.Nombres = persona.RazonSocialONombresCompletos;
            userRequest.Apellidos = string.Empty;
            userRequest.Email = persona.CorreoElectronicoPrincipal;
            userRequest.Telefono = persona.TelefonoCelular;
            userRequest.Direccion = persona.DireccionPrincipal;
            userRequest.IsActive = persona.IsActive;

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

    private async Task SaveUserAsync()
    {
        isSaving = true;
        errorMessage = null;
        userRequest.Roles = selectedRoles.ToArray();

        try
        {
            var result = editingUserId.HasValue
                ? await SecurityApiClient.UpdateUserAsync(editingUserId.Value, userRequest)
                : await SecurityApiClient.CreateUserAsync(userRequest);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseModal();
            await LoadUsersAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar el usuario.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private Task SearchAsync() => LoadUsersAsync(resetPaging: true);

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadUsersAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadUsersAsync();
    }
}
