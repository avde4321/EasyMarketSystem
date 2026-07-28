using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Shared.Requests.Catalogos;
using TestDeIa.Shared.Responses.Catalogos;

namespace TestDeIa.Client.Pages;

public partial class Catalogos
{
    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    private readonly List<CatalogoResponse> catalogos = [];
    private readonly List<CatalogoItemResponse> catalogoItems = [];
    private CatalogoResponse? selectedCatalogo;
    private CatalogoItemRequest catalogoItemRequest = new();
    private Guid? editingItemId;
    private bool isLoadingCatalogos = true;
    private bool isLoadingItems;
    private bool isEditorOpen;
    private bool isSaving;
    private string? errorMessage;
    private string catalogSearchTerm = string.Empty;
    private string itemSearchTerm = string.Empty;

    private IEnumerable<CatalogoResponse> FilteredCatalogos => catalogos.Where(catalogo =>
        string.IsNullOrWhiteSpace(catalogSearchTerm) ||
        catalogo.Codigo.Contains(catalogSearchTerm, StringComparison.OrdinalIgnoreCase) ||
        catalogo.Nombre.Contains(catalogSearchTerm, StringComparison.OrdinalIgnoreCase) ||
        (catalogo.Descripcion?.Contains(catalogSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

    private IEnumerable<CatalogoItemResponse> FilteredCatalogoItems => catalogoItems.Where(item =>
        string.IsNullOrWhiteSpace(itemSearchTerm) ||
        item.Codigo.Contains(itemSearchTerm, StringComparison.OrdinalIgnoreCase) ||
        item.Nombre.Contains(itemSearchTerm, StringComparison.OrdinalIgnoreCase) ||
        (item.Descripcion?.Contains(itemSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
    }

    private async Task LoadCatalogosAsync()
    {
        isLoadingCatalogos = true;
        errorMessage = null;

        try
        {
            catalogos.Clear();
            catalogos.AddRange(await CatalogosApiClient.GetCatalogosAsync());
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el modulo de catalogos.";
        }
        finally
        {
            isLoadingCatalogos = false;
        }
    }

    private async Task SelectCatalogoAsync(CatalogoResponse catalogo)
    {
        selectedCatalogo = catalogo;
        await LoadItemsAsync();
    }

    private async Task LoadItemsAsync()
    {
        if (selectedCatalogo is null)
        {
            return;
        }

        isLoadingItems = true;
        errorMessage = null;

        try
        {
            catalogoItems.Clear();
            catalogoItems.AddRange(await CatalogosApiClient.GetItemsAsync(selectedCatalogo.Codigo));
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el detalle del catalogo.";
        }
        finally
        {
            isLoadingItems = false;
        }
    }

    private void OpenCreateModal()
    {
        if (selectedCatalogo is null)
        {
            return;
        }

        editingItemId = null;
        catalogoItemRequest = new CatalogoItemRequest
        {
            CatalogoCodigo = selectedCatalogo.Codigo,
            IsActive = true
        };
        isEditorOpen = true;
    }

    private void OpenEditModal(CatalogoItemResponse item)
    {
        editingItemId = item.Id;
        catalogoItemRequest = new CatalogoItemRequest
        {
            CatalogoCodigo = item.CatalogoCodigo,
            Codigo = item.Codigo,
            Nombre = item.Nombre,
            Descripcion = item.Descripcion,
            Orden = item.Orden,
            IsActive = item.IsActive
        };
        isEditorOpen = true;
    }

    private void CloseModal()
    {
        isEditorOpen = false;
        isSaving = false;
        errorMessage = null;
    }

    private async Task SaveAsync()
    {
        isSaving = true;
        errorMessage = null;

        try
        {
            catalogoItemRequest.CatalogoCodigo = selectedCatalogo?.Codigo ?? catalogoItemRequest.CatalogoCodigo;

            var result = editingItemId.HasValue
                ? await CatalogosApiClient.UpdateItemAsync(editingItemId.Value, catalogoItemRequest)
                : await CatalogosApiClient.CreateItemAsync(catalogoItemRequest);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseModal();
            await LoadCatalogosAsync();
            if (selectedCatalogo is not null)
            {
                selectedCatalogo = catalogos.FirstOrDefault(catalogo => catalogo.Codigo == selectedCatalogo.Codigo);
                await LoadItemsAsync();
            }
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar el item del catalogo.";
        }
        finally
        {
            isSaving = false;
        }
    }
}
