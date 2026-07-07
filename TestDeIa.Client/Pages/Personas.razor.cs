using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Shared.Requests.Personas;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Personas;

namespace TestDeIa.Client.Pages;

public partial class Personas
{
    [Inject]
    private PersonasApiClient PersonasApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    private readonly List<PersonaResponse> personas = [];
    private readonly List<CatalogoItemResponse> tiposIdentificacion = [];
    private PersonaRequest personaRequest = new();
    private Guid? editingPersonaId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isEditorOpen;
    private string? errorMessage;
    private string searchTerm = string.Empty;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;
    private IEnumerable<PersonaResponse> VisiblePersonas => personas;
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
        await LoadPersonasAsync(resetPaging: true);
    }

    private async Task LoadCatalogosAsync()
    {
        tiposIdentificacion.Clear();
        tiposIdentificacion.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_IDENTIFICACION", true));
    }

    private async Task LoadPersonasAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var page = await PersonasApiClient.GetPagedAsync(searchTerm, currentSkip, PageSize);
            personas.Clear();
            personas.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar la lista de personas.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenCreateModal()
    {
        editingPersonaId = null;
        personaRequest = new PersonaRequest
        {
            TipoIdentificacion = tiposIdentificacion.FirstOrDefault()?.Codigo ?? "05"
        };
        errorMessage = null;
        isEditorOpen = true;
    }

    private void OpenEditModal(PersonaResponse persona)
    {
        editingPersonaId = persona.Id;
        errorMessage = null;
        personaRequest = new PersonaRequest
        {
            TipoIdentificacion = persona.TipoIdentificacion,
            Identificacion = persona.Identificacion,
            RazonSocialONombresCompletos = persona.RazonSocialONombresCompletos,
            NombreComercial = persona.NombreComercial,
            FechaNacimiento = persona.FechaNacimiento,
            CorreoElectronicoPrincipal = persona.CorreoElectronicoPrincipal,
            TelefonoCelular = persona.TelefonoCelular,
            DireccionPrincipal = persona.DireccionPrincipal,
            Genero = persona.Genero,
            IsActive = persona.IsActive
        };
        isEditorOpen = true;
    }

    private void CloseModal()
    {
        isEditorOpen = false;
        isSaving = false;
        errorMessage = null;
    }

    private async Task SavePersonaAsync()
    {
        isSaving = true;
        errorMessage = null;

        try
        {
            var result = editingPersonaId.HasValue
                ? await PersonasApiClient.UpdateAsync(editingPersonaId.Value, personaRequest)
                : await PersonasApiClient.CreateAsync(personaRequest);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseModal();
            await LoadPersonasAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar la persona.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private Task SearchAsync() => LoadPersonasAsync(resetPaging: true);

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadPersonasAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadPersonasAsync();
    }
}
