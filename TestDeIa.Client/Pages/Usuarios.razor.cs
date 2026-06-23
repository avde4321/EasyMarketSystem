using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Security;
using TestDeIa.Client.Services.Catalogos;
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

    private readonly List<SecurityUserResponse> users = [];
    private readonly List<SecurityRoleResponse> roles = [];
    private readonly List<CatalogoItemResponse> tiposIdentificacion = [];
    private readonly HashSet<string> selectedRoles = new(StringComparer.OrdinalIgnoreCase);
    private SecurityUserRequest userRequest = new();
    private Guid? editingUserId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isEditorOpen;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadLookupsAsync();
        await LoadUsersAsync();
    }

    private async Task LoadLookupsAsync()
    {
        roles.Clear();
        tiposIdentificacion.Clear();
        roles.AddRange(await SecurityApiClient.GetRolesAsync());
        tiposIdentificacion.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_IDENTIFICACION", true));
    }

    private async Task LoadUsersAsync()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            users.Clear();
            users.AddRange(await SecurityApiClient.GetUsersAsync());
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
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "Cedula",
            IsActive = true
        };
        selectedRoles.Clear();
        errorMessage = null;
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
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "Cedula",
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
}
