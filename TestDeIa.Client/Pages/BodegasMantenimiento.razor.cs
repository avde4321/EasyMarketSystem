using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Inventario;
using TestDeIa.Shared.Requests.Inventario;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Client.Pages;

public partial class BodegasMantenimiento
{
    [Inject]
    private InventarioApiClient InventarioApiClient { get; set; } = default!;

    private readonly List<BodegaResponse> bodegas = [];
    private BodegaRequest bodegaRequest = new();
    private Guid? editingBodegaId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isModalOpen;
    private string? dialogMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        isLoading = true;
        try
        {
            bodegas.Clear();
            bodegas.AddRange((await InventarioApiClient.GetBodegasAsync())
                .OrderByDescending(bodega => bodega.EsPrincipal)
                .ThenBy(bodega => bodega.Codigo));
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenCreateModal()
    {
        editingBodegaId = null;
        bodegaRequest = new BodegaRequest
        {
            Codigo = SuggestNextCode(),
            IsActive = true
        };
        dialogMessage = null;
        isModalOpen = true;
    }

    private void OpenEditModal(BodegaResponse bodega)
    {
        editingBodegaId = bodega.Id;
        bodegaRequest = new BodegaRequest
        {
            Codigo = bodega.Codigo,
            Nombre = bodega.Nombre,
            Direccion = bodega.Direccion,
            EsPrincipal = bodega.EsPrincipal,
            IsActive = bodega.IsActive
        };
        dialogMessage = null;
        isModalOpen = true;
    }

    private void CloseModal()
    {
        isModalOpen = false;
        isSaving = false;
        dialogMessage = null;
    }

    private async Task SaveAsync()
    {
        isSaving = true;
        dialogMessage = null;

        try
        {
            var result = editingBodegaId.HasValue
                ? await InventarioApiClient.UpdateBodegaAsync(editingBodegaId.Value, bodegaRequest)
                : await InventarioApiClient.CreateBodegaAsync(bodegaRequest);

            if (!result.Succeeded)
            {
                dialogMessage = result.ErrorMessage ?? "No se pudo guardar la bodega.";
                return;
            }

            CloseModal();
            await LoadAsync();
        }
        finally
        {
            isSaving = false;
        }
    }

    private string SuggestNextCode()
    {
        var maxCode = bodegas
            .Select(bodega => int.TryParse(bodega.Codigo, out var code) ? code : 0)
            .DefaultIfEmpty(0)
            .Max();

        return Math.Min(maxCode + 1, 999).ToString("000");
    }
}
