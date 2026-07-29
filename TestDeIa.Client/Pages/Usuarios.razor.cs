using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Security;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Empresa;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Client.Services.Security;
using TestDeIa.Client.Services;
using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Personas;
using TestDeIa.Shared.Responses.Security;
using TestDeIa.Shared.Security;

namespace TestDeIa.Client.Pages;

public partial class Usuarios
{
    [Inject]
    private SecurityApiClient SecurityApiClient { get; set; } = default!;

    [Inject]
    private HttpClient HttpClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    [Inject]
    private PersonasApiClient PersonasApiClient { get; set; } = default!;

    [Inject]
    private EmpresaApiClient EmpresaApiClient { get; set; } = default!;

    [Inject]
    private PopupNotificationService PopupNotificationService { get; set; } = default!;

    private SecurityUserAdminApiClient UserAdminApiClient => new(HttpClient);

    private readonly List<SecurityUserResponse> users = [];
    private readonly List<SecurityRoleResponse> roles = [];
    private readonly List<CatalogoItemResponse> tiposIdentificacion = [];
    private readonly List<SecurityPointEmissionResponse> puntosEmisionDisponibles = [];
    private readonly List<PersonaResponse> personaSearchResults = [];
    private readonly HashSet<string> selectedRoles = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> profileSelectedRoles = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<Guid> selectedPuntoEmisionIds = [];
    private SecurityUserAdminRequest userRequest = new();
    private ResetPasswordRequest resetPasswordRequest = new();
    private Guid? editingUserId;
    private Guid? resetUserId;
    private Guid? unlockUserId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isResettingPassword;
    private bool isUnlocking;
    private bool isSavingProfile;
    private bool isChangingState;
    private bool isEditorOpen;
    private bool isResetModalOpen;
    private bool isUnlockModalOpen;
    private bool isStateModalOpen;
    private bool isSearchingPersona;
    private bool isSearchingPersonas;
    private bool isManualUserEntry;
    private string? errorMessage;
    private string? statusMessage;
    private string? profileErrorMessage;
    private string searchTerm = string.Empty;
    private string personaSearchTerm = string.Empty;
    private string? pendingState;
    private string currentEmpresaLabel = "Sin empresa activa";
    private SecurityUserResponse? profileUser;
    private PersonaResponse? selectedPersonaForUser;
    private string stateConfirmationMessage = string.Empty;
    private const int PageSize = 10;
    private const int PersonaLookupPageSize = 5;
    private int totalCount;
    private int currentSkip;
    private int personaLookupTotalCount;
    private int personaLookupSkip;
    private IEnumerable<SecurityUserResponse> VisibleUsers => users;
    private IReadOnlyCollection<string> SelectedPermissions => roles
        .Where(role => profileSelectedRoles.Contains(role.Name))
        .SelectMany(role => role.Permissions)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(permission => permission)
        .ToArray();
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
    private bool RequiresPuntosEmision => selectedRoles.Contains(SecurityRoleNames.Cajero);
    private bool CanGoPreviousPersonaLookup => personaLookupSkip > 0;
    private bool CanGoNextPersonaLookup => personaLookupSkip + PersonaLookupPageSize < personaLookupTotalCount;
    private int PersonaLookupPageNumber => (personaLookupSkip / PersonaLookupPageSize) + 1;
    private int PersonaLookupTotalPages => Math.Max(1, (int)Math.Ceiling(personaLookupTotalCount / (double)PersonaLookupPageSize));

    protected override async Task OnInitializedAsync()
    {
        await LoadLookupsAsync();
        await LoadCurrentEmpresaAsync();
        await LoadUsersAsync(resetPaging: true);
    }

    private async Task LoadLookupsAsync()
    {
        roles.Clear();
        tiposIdentificacion.Clear();
        puntosEmisionDisponibles.Clear();

        roles.AddRange(await SecurityApiClient.GetRolesAsync());
        tiposIdentificacion.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_IDENTIFICACION", true));
        puntosEmisionDisponibles.AddRange(await UserAdminApiClient.GetPuntosEmisionAsync());
    }

    private async Task LoadCurrentEmpresaAsync()
    {
        try
        {
            var empresa = await EmpresaApiClient.GetCurrentAsync();
            currentEmpresaLabel = empresa is null
                ? "Sin empresa activa"
                : $"{empresa.RazonSocial} - {empresa.Ruc}";
        }
        catch (HttpRequestException)
        {
            currentEmpresaLabel = "Sin empresa activa";
        }
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
            await PopupNotificationService.ShowErrorAsync(errorMessage);
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task OpenCreateModal()
    {
        editingUserId = null;
        userRequest = new SecurityUserAdminRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "05",
            IsActive = true
        };
        selectedRoles.Clear();
        selectedPuntoEmisionIds.Clear();
        personaSearchResults.Clear();
        selectedPersonaForUser = null;
        personaSearchTerm = string.Empty;
        personaLookupSkip = 0;
        personaLookupTotalCount = 0;
        isManualUserEntry = false;
        errorMessage = null;
        statusMessage = null;
        isEditorOpen = true;
        await LoadPersonasForUserLookupAsync(resetPaging: true, showEmptyPopup: false);
    }

    private async Task OpenEditModalAsync(SecurityUserResponse user)
    {
        editingUserId = user.Id;
        var persona = await PersonasApiClient.FindByIdentificacionAsync(user.PersonaIdentificacion);
        var tipoIdentificacion = persona?.TipoIdentificacion ?? tiposIdentificacion.FirstOrDefault()?.Codigo ?? "05";
        var identificacion = persona?.Identificacion ?? user.PersonaIdentificacion;
        var (nombres, apellidos) = SplitDisplayName(persona?.RazonSocialONombresCompletos ?? user.PersonaNombre);
        var email = persona?.CorreoElectronicoPrincipal ?? user.Email;
        var telefono = persona?.TelefonoCelular;
        var direccion = persona?.DireccionPrincipal;
        var isActive = persona?.IsActive ?? user.IsActive;

        userRequest = new SecurityUserAdminRequest
        {
            TipoIdentificacion = tipoIdentificacion,
            Identificacion = identificacion,
            Nombres = nombres,
            Apellidos = apellidos,
            Email = email,
            Telefono = telefono,
            Direccion = direccion,
            UserName = user.UserName,
            IsActive = isActive,
            Roles = user.Roles.ToArray()
        };

        selectedRoles.Clear();
        foreach (var role in user.Roles)
        {
            selectedRoles.Add(role);
        }

        selectedPuntoEmisionIds.Clear();
        foreach (var puntoEmisionId in await UserAdminApiClient.GetPuntosEmisionByUserAsync(user.Id))
        {
            selectedPuntoEmisionIds.Add(puntoEmisionId);
        }

        errorMessage = null;
        statusMessage = null;
        selectedPersonaForUser = null;
        personaSearchResults.Clear();
        isManualUserEntry = false;
        isEditorOpen = true;
    }

    private void OpenResetPasswordModal(SecurityUserResponse user)
    {
        resetUserId = user.Id;
        resetPasswordRequest = new ResetPasswordRequest();
        statusMessage = null;
        errorMessage = null;
        isResetModalOpen = true;
    }

    private void OpenUnlockModal(SecurityUserResponse user)
    {
        unlockUserId = user.Id;
        statusMessage = null;
        errorMessage = null;
        isUnlockModalOpen = true;
    }

    private void OpenProfileModal(SecurityUserResponse user)
    {
        profileUser = user;
        profileSelectedRoles.Clear();
        foreach (var role in user.Roles)
        {
            profileSelectedRoles.Add(role);
        }

        profileErrorMessage = null;
        pendingState = null;
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
            if (string.Equals(roleName, SecurityRoleNames.Cajero, StringComparison.OrdinalIgnoreCase))
            {
                selectedPuntoEmisionIds.Clear();
            }
        }
    }

    private void TogglePuntoEmision(Guid puntoEmisionId, object? value)
    {
        var isChecked = value as bool? == true;

        if (isChecked)
        {
            selectedPuntoEmisionIds.Add(puntoEmisionId);
        }
        else
        {
            selectedPuntoEmisionIds.Remove(puntoEmisionId);
        }
    }

    private void CloseModal()
    {
        isEditorOpen = false;
        isSaving = false;
        errorMessage = null;
        statusMessage = null;
        selectedPuntoEmisionIds.Clear();
        selectedPersonaForUser = null;
        personaSearchResults.Clear();
        personaSearchTerm = string.Empty;
        isManualUserEntry = false;
    }

    private void CloseResetModal()
    {
        isResetModalOpen = false;
        isResettingPassword = false;
        resetUserId = null;
    }

    private void CloseUnlockModal()
    {
        isUnlockModalOpen = false;
        isUnlocking = false;
        unlockUserId = null;
    }

    private Task CloseProfileModal()
    {
        isSavingProfile = false;
        profileErrorMessage = null;
        profileUser = null;
        pendingState = null;
        isStateModalOpen = false;
        return Task.CompletedTask;
    }

    private void CloseStateModal()
    {
        isStateModalOpen = false;
        isChangingState = false;
        pendingState = null;
    }

    private async Task SearchPersonasForUserAsync()
    {
        await LoadPersonasForUserLookupAsync(resetPaging: true, showEmptyPopup: true);
    }

    private async Task LoadPersonasForUserLookupAsync(bool resetPaging, bool showEmptyPopup)
    {
        isSearchingPersonas = true;
        errorMessage = null;
        statusMessage = null;
        personaSearchResults.Clear();

        if (resetPaging)
        {
            personaLookupSkip = 0;
        }

        try
        {
            var page = await PersonasApiClient.GetPagedAsync(personaSearchTerm, personaLookupSkip, PersonaLookupPageSize);
            personaSearchResults.AddRange(page.Items);
            personaLookupTotalCount = page.TotalCount;
            if (personaSearchResults.Count == 0)
            {
                statusMessage = "No se encontraron personas con ese filtro. Puedes usar ingreso manual.";
                if (showEmptyPopup)
                {
                    await PopupNotificationService.ShowInfoAsync(statusMessage);
                }
            }
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo consultar personas para convertir en usuario.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
        }
        finally
        {
            isSearchingPersonas = false;
        }
    }

    private async Task GoToPreviousPersonaLookupAsync()
    {
        if (!CanGoPreviousPersonaLookup)
        {
            return;
        }

        personaLookupSkip = Math.Max(0, personaLookupSkip - PersonaLookupPageSize);
        await LoadPersonasForUserLookupAsync(resetPaging: false, showEmptyPopup: false);
    }

    private async Task GoToNextPersonaLookupAsync()
    {
        if (!CanGoNextPersonaLookup)
        {
            return;
        }

        personaLookupSkip += PersonaLookupPageSize;
        await LoadPersonasForUserLookupAsync(resetPaging: false, showEmptyPopup: false);
    }

    private async Task SelectPersonaForUser(PersonaResponse persona)
    {
        selectedPersonaForUser = persona;
        isManualUserEntry = false;
        FillUserRequestFromPersona(persona);
        statusMessage = "Persona seleccionada. Completa usuario, clave y roles para guardar el acceso.";
        await PopupNotificationService.ShowSuccessAsync(statusMessage);
    }

    private async Task StartManualUserEntry()
    {
        selectedPersonaForUser = null;
        personaSearchResults.Clear();
        isManualUserEntry = true;
        statusMessage = "Ingreso manual habilitado. Completa los datos base y de acceso.";
        await PopupNotificationService.ShowInfoAsync(statusMessage);
    }

    private void ReturnToPersonaSelection()
    {
        selectedPersonaForUser = null;
        isManualUserEntry = false;
        statusMessage = null;
    }

    private void FillUserRequestFromPersona(PersonaResponse persona)
    {
        var (nombres, apellidos) = SplitDisplayName(persona.RazonSocialONombresCompletos);
        userRequest.TipoIdentificacion = persona.TipoIdentificacion;
        userRequest.Identificacion = persona.Identificacion;
        userRequest.Nombres = nombres;
        userRequest.Apellidos = apellidos;
        userRequest.Email = persona.CorreoElectronicoPrincipal;
        userRequest.Telefono = persona.TelefonoCelular;
        userRequest.Direccion = persona.DireccionPrincipal;
        userRequest.IsActive = persona.IsActive;

        if (string.IsNullOrWhiteSpace(userRequest.UserName))
        {
            userRequest.UserName = BuildUserNameSuggestion(persona);
        }
    }

    private static bool TieneRolUsuario(PersonaResponse persona)
    {
        return persona.RolesPersona.Any(role => string.Equals(role, "Usuario", StringComparison.OrdinalIgnoreCase));
    }

    private async Task BuscarPersonaAsync()
    {
        errorMessage = null;
        statusMessage = null;

        if (string.IsNullOrWhiteSpace(userRequest.Identificacion))
        {
            errorMessage = "Ingresa una identificacion antes de buscar.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
            return;
        }

        isSearchingPersona = true;

        try
        {
            var persona = await PersonasApiClient.FindByIdentificacionAsync(userRequest.Identificacion);
            if (persona is null)
            {
                statusMessage = "No se encontro una persona registrada con esa identificacion.";
                await PopupNotificationService.ShowInfoAsync(statusMessage);
                return;
            }

            userRequest.TipoIdentificacion = persona.TipoIdentificacion;
            userRequest.Identificacion = persona.Identificacion;
            var (nombres, apellidos) = SplitDisplayName(persona.RazonSocialONombresCompletos);
            userRequest.Nombres = nombres;
            userRequest.Apellidos = apellidos;
            userRequest.Email = persona.CorreoElectronicoPrincipal;
            userRequest.Telefono = persona.TelefonoCelular;
            userRequest.Direccion = persona.DireccionPrincipal;
            userRequest.IsActive = persona.IsActive;

            statusMessage = "Se cargo la informacion de la persona existente.";
            await PopupNotificationService.ShowSuccessAsync(statusMessage);
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo consultar la persona.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
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
        userRequest.PuntoEmisionIds = selectedPuntoEmisionIds.ToArray();

        if (RequiresPuntosEmision && userRequest.PuntoEmisionIds.Count == 0)
        {
            errorMessage = "Si el usuario tiene el rol Cajero debes asignar al menos un punto de emision.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
            isSaving = false;
            return;
        }

        try
        {
            var result = editingUserId.HasValue
                ? await UserAdminApiClient.UpdateAsync(editingUserId.Value, userRequest)
                : await UserAdminApiClient.CreateAsync(userRequest);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                await PopupNotificationService.ShowErrorAsync(errorMessage ?? "No se pudo guardar el usuario.");
                return;
            }

            CloseModal();
            await PopupNotificationService.ShowSuccessAsync("Usuario guardado correctamente.");
            await LoadUsersAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar el usuario.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
        }
        finally
        {
            isSaving = false;
        }
    }

    private Task ToggleProfileRoleAsync(string roleName)
    {
        if (profileSelectedRoles.Contains(roleName))
        {
            profileSelectedRoles.Remove(roleName);
        }
        else
        {
            profileSelectedRoles.Add(roleName);
        }

        return Task.CompletedTask;
    }

    private Task OpenStateModalAsync(string state)
    {
        if (profileUser is null)
        {
            return Task.CompletedTask;
        }

        pendingState = state;
        stateConfirmationMessage = state switch
        {
            SecurityUserEstados.Bloqueado => $"Se bloqueara la cuenta de {profileUser.DisplayName} y se cerraran sus sesiones activas.",
            SecurityUserEstados.Inactivo => $"La cuenta de {profileUser.DisplayName} quedara inactiva y sus tokens dejaran de ser validos.",
            _ => $"La cuenta de {profileUser.DisplayName} volvera a estado activo."
        };
        isStateModalOpen = true;
        return Task.CompletedTask;
    }

    private Task OpenToggleStateModal(SecurityUserResponse user)
    {
        profileUser = user;
        var targetState = user.Estado == SecurityUserEstados.Activo
            ? SecurityUserEstados.Inactivo
            : SecurityUserEstados.Activo;

        return OpenStateModalAsync(targetState);
    }

    private async Task SaveProfileAsync()
    {
        if (profileUser is null)
        {
            return;
        }

        isSavingProfile = true;
        profileErrorMessage = null;

        try
        {
            var result = await SecurityApiClient.UpdatePerfilAsync(profileUser.Id, new UpdateUserPerfilRequest
            {
                Roles = profileSelectedRoles.ToArray()
            });

            if (!result.Succeeded)
            {
                profileErrorMessage = result.ErrorMessage ?? "No se pudo actualizar el perfil.";
                await PopupNotificationService.ShowErrorAsync(profileErrorMessage);
                return;
            }

            await LoadUsersAsync();
            profileUser = users.FirstOrDefault(current => current.Id == profileUser.Id);
            statusMessage = "Perfil actualizado correctamente.";
            await PopupNotificationService.ShowSuccessAsync(statusMessage);

            if (profileUser is null)
            {
                await CloseProfileModal();
            }
        }
        catch (HttpRequestException)
        {
            profileErrorMessage = "No se pudo actualizar el perfil del usuario.";
            await PopupNotificationService.ShowErrorAsync(profileErrorMessage);
        }
        finally
        {
            isSavingProfile = false;
        }
    }

    private async Task ConfirmStateChangeAsync()
    {
        if (profileUser is null || string.IsNullOrWhiteSpace(pendingState))
        {
            return;
        }

        isChangingState = true;
        profileErrorMessage = null;

        try
        {
            var result = await SecurityApiClient.UpdateEstadoAsync(profileUser.Id, new UpdateUserEstadoRequest
            {
                Estado = pendingState
            });

            if (!result.Succeeded)
            {
                profileErrorMessage = result.ErrorMessage ?? "No se pudo cambiar el estado del usuario.";
                await PopupNotificationService.ShowErrorAsync(profileErrorMessage);
                return;
            }

            CloseStateModal();
            await LoadUsersAsync();
            profileUser = users.FirstOrDefault(current => current.Id == profileUser.Id);
            statusMessage = "Estado del usuario actualizado correctamente.";
            await PopupNotificationService.ShowSuccessAsync(statusMessage);
        }
        catch (HttpRequestException)
        {
            profileErrorMessage = "No se pudo cambiar el estado del usuario.";
            await PopupNotificationService.ShowErrorAsync(profileErrorMessage);
        }
        finally
        {
            isChangingState = false;
        }
    }

    private async Task ConfirmResetPasswordAsync()
    {
        if (!resetUserId.HasValue)
        {
            return;
        }

        isResettingPassword = true;
        errorMessage = null;

        try
        {
            var result = await SecurityApiClient.ResetPasswordAsync(resetUserId.Value, resetPasswordRequest);
            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                await PopupNotificationService.ShowErrorAsync(errorMessage ?? "No se pudo resetear la clave.");
                return;
            }

            CloseResetModal();
            statusMessage = "Clave temporal actualizada correctamente.";
            await PopupNotificationService.ShowSuccessAsync(statusMessage);
            await LoadUsersAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo resetear la clave.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
        }
        finally
        {
            isResettingPassword = false;
        }
    }

    private async Task ConfirmUnlockAsync()
    {
        if (!unlockUserId.HasValue)
        {
            return;
        }

        isUnlocking = true;
        errorMessage = null;

        try
        {
            var result = await SecurityApiClient.UnlockUserAsync(unlockUserId.Value);
            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                await PopupNotificationService.ShowErrorAsync(errorMessage ?? "No se pudo desbloquear el usuario.");
                return;
            }

            CloseUnlockModal();
            statusMessage = "Usuario desbloqueado correctamente.";
            await PopupNotificationService.ShowSuccessAsync(statusMessage);
            await LoadUsersAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo desbloquear el usuario.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
        }
        finally
        {
            isUnlocking = false;
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

    private static string GetStatusClass(SecurityUserResponse user)
        => user.Estado switch
        {
            SecurityUserEstados.Activo => "status-pill success-pill",
            SecurityUserEstados.Bloqueado => "status-pill danger-pill",
            _ => "status-pill soft-pill"
        };

    private static (string Nombres, string Apellidos) SplitDisplayName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return ("Usuario", "-");
        }

        if (parts.Length == 1)
        {
            return (parts[0], "-");
        }

        if (parts.Length == 2)
        {
            return (parts[0], parts[1]);
        }

        var splitIndex = Math.Max(1, parts.Length - 2);
        return (string.Join(' ', parts.Take(splitIndex)), string.Join(' ', parts.Skip(splitIndex)));
    }

    private static string BuildUserNameSuggestion(PersonaResponse persona)
    {
        if (!string.IsNullOrWhiteSpace(persona.CorreoElectronicoPrincipal))
        {
            return persona.CorreoElectronicoPrincipal.Split('@')[0].Trim();
        }

        var normalized = new string(persona.Identificacion.Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrWhiteSpace(normalized) ? "usuario" : normalized.ToLowerInvariant();
    }
}










