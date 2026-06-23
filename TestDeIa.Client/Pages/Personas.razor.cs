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
    private readonly List<CatalogoItemResponse> estadosCiviles = [];
    private PersonaRequest personaRequest = new();
    private Guid? editingPersonaId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isEditorOpen;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
        await LoadPersonasAsync();
    }

    private async Task LoadCatalogosAsync()
    {
        tiposIdentificacion.Clear();
        estadosCiviles.Clear();
        tiposIdentificacion.AddRange(await CatalogosApiClient.GetItemsAsync("TIPO_IDENTIFICACION", true));
        estadosCiviles.AddRange(await CatalogosApiClient.GetItemsAsync("ESTADO_CIVIL", true));
    }

    private async Task LoadPersonasAsync()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            personas.Clear();
            personas.AddRange(await PersonasApiClient.GetAllAsync());
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
        personaRequest = new PersonaRequest();
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
            Nombres = persona.Nombres,
            Apellidos = persona.Apellidos,
            EstadoCivil = persona.EstadoCivil,
            FechaNacimiento = persona.FechaNacimiento,
            Email = persona.Email,
            Telefono = persona.Telefono,
            Direccion = persona.Direccion,
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
}
