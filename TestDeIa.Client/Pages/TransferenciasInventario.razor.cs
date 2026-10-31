using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Inventario;
using TestDeIa.Shared.Requests.Inventario;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Client.Pages;

public partial class TransferenciasInventario
{
    [Inject]
    private InventarioApiClient InventarioApiClient { get; set; } = default!;

    private readonly List<BodegaResponse> bodegas = [];
    private readonly List<ProductoResponse> productosOrigen = [];
    private readonly List<TransferenciaInventarioResponse> transferencias = [];
    private TransferenciaInventarioFormalRequest transferenciaRequest = new();
    private RecepcionTransferenciaInventarioRequest recepcionRequest = new();
    private TransferenciaInventarioResponse? selectedTransferencia;
    private bool isLoading = true;
    private bool isSaving;
    private bool isCreateModalOpen;
    private bool isRecepcionModalOpen;
    private string estadoFiltro = string.Empty;
    private string origenFiltro = string.Empty;
    private string destinoFiltro = string.Empty;
    private string? dialogMessage;
    private Guid selectedProductoId;
    private decimal selectedCantidad = 1;

    private IReadOnlyList<BodegaResponse> activeBodegas => bodegas.Where(bodega => bodega.IsActive).ToList();
    private Guid? origenFiltroId => Guid.TryParse(origenFiltro, out var id) ? id : null;
    private Guid? destinoFiltroId => Guid.TryParse(destinoFiltro, out var id) ? id : null;
    private bool CanSaveTransferencia =>
        !isSaving &&
        transferenciaRequest.BodegaOrigenId != Guid.Empty &&
        transferenciaRequest.BodegaDestinoId != Guid.Empty &&
        transferenciaRequest.BodegaOrigenId != transferenciaRequest.BodegaDestinoId &&
        transferenciaRequest.Detalles.Count > 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadBodegasAsync();
        await LoadTransferenciasAsync();
    }

    private async Task LoadBodegasAsync()
    {
        bodegas.Clear();
        bodegas.AddRange(await InventarioApiClient.GetBodegasAsync());
    }

    private async Task LoadTransferenciasAsync()
    {
        isLoading = true;
        try
        {
            transferencias.Clear();
            transferencias.AddRange(await InventarioApiClient.GetTransferenciasAsync(
                estadoFiltro,
                origenFiltroId,
                destinoFiltroId));
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenCreateModal()
    {
        transferenciaRequest = new TransferenciaInventarioFormalRequest
        {
            MotivoTraslado = "Transferencia interna"
        };
        productosOrigen.Clear();
        selectedProductoId = Guid.Empty;
        selectedCantidad = 1;
        dialogMessage = null;
        isCreateModalOpen = true;
    }

    private void CloseCreateModal()
    {
        isCreateModalOpen = false;
        isSaving = false;
        dialogMessage = null;
    }

    private async Task OnOrigenChangedAsync()
    {
        transferenciaRequest.Detalles.Clear();
        productosOrigen.Clear();
        selectedProductoId = Guid.Empty;

        if (transferenciaRequest.BodegaOrigenId == Guid.Empty)
        {
            return;
        }

        var page = await InventarioApiClient.GetProductosAsync(null, 0, 50, transferenciaRequest.BodegaOrigenId);
        productosOrigen.AddRange(page.Items.Where(producto => producto.ControlaStock && producto.StockActual > 0));
    }

    private void AddDetalle()
    {
        dialogMessage = null;
        var producto = productosOrigen.FirstOrDefault(current => current.Id == selectedProductoId);
        if (producto is null)
        {
            dialogMessage = "Selecciona un producto con stock en la bodega origen.";
            return;
        }

        if (selectedCantidad <= 0 || selectedCantidad > producto.StockActual)
        {
            dialogMessage = $"La cantidad debe ser mayor a cero y no superar el stock disponible ({producto.StockActual:0.####}).";
            return;
        }

        var existing = transferenciaRequest.Detalles.FirstOrDefault(current => current.ProductoId == producto.Id);
        if (existing is not null)
        {
            var nuevaCantidad = existing.Cantidad + selectedCantidad;
            if (nuevaCantidad > producto.StockActual)
            {
                dialogMessage = $"La cantidad acumulada no puede superar el stock disponible ({producto.StockActual:0.####}).";
                return;
            }

            existing.Cantidad = nuevaCantidad;
        }
        else
        {
            transferenciaRequest.Detalles.Add(new TransferenciaInventarioDetalleRequest
            {
                ProductoId = producto.Id,
                Cantidad = selectedCantidad
            });
        }

        selectedProductoId = Guid.Empty;
        selectedCantidad = 1;
    }

    private void RemoveDetalle(TransferenciaInventarioDetalleRequest detalle)
    {
        transferenciaRequest.Detalles.Remove(detalle);
    }

    private async Task SaveTransferenciaAsync()
    {
        if (!CanSaveTransferencia)
        {
            return;
        }

        isSaving = true;
        dialogMessage = null;
        try
        {
            var result = await InventarioApiClient.CreateTransferenciaAsync(transferenciaRequest);
            if (!result.Succeeded)
            {
                dialogMessage = result.ErrorMessage ?? "No se pudo crear la transferencia.";
                return;
            }

            CloseCreateModal();
            await LoadTransferenciasAsync();
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DespacharAsync(TransferenciaInventarioResponse transferencia)
    {
        isSaving = true;
        try
        {
            var result = await InventarioApiClient.DespacharTransferenciaAsync(transferencia.Id);
            if (!result.Succeeded)
            {
                dialogMessage = result.ErrorMessage ?? "No se pudo despachar la transferencia.";
                return;
            }

            await LoadTransferenciasAsync();
        }
        finally
        {
            isSaving = false;
        }
    }

    private void OpenRecepcionModal(TransferenciaInventarioResponse transferencia)
    {
        selectedTransferencia = transferencia;
        recepcionRequest = new RecepcionTransferenciaInventarioRequest
        {
            Detalles = transferencia.Detalles
                .Select(detalle => new RecepcionTransferenciaInventarioDetalleRequest
                {
                    DetalleId = detalle.Id,
                    CantidadRecibida = detalle.CantidadEnviada
                })
                .ToList()
        };
        dialogMessage = null;
        isRecepcionModalOpen = true;
    }

    private void CloseRecepcionModal()
    {
        isRecepcionModalOpen = false;
        selectedTransferencia = null;
        dialogMessage = null;
        isSaving = false;
    }

    private async Task RecibirAsync()
    {
        if (selectedTransferencia is null)
        {
            return;
        }

        isSaving = true;
        dialogMessage = null;
        try
        {
            var result = await InventarioApiClient.RecibirTransferenciaAsync(selectedTransferencia.Id, recepcionRequest);
            if (!result.Succeeded)
            {
                dialogMessage = result.ErrorMessage ?? "No se pudo confirmar la recepcion.";
                return;
            }

            CloseRecepcionModal();
            await LoadTransferenciasAsync();
        }
        finally
        {
            isSaving = false;
        }
    }

    private static string FormatEstado(string estado) => estado switch
    {
        "EnTransito" => "En transito",
        _ => estado
    };

    private static string GetEstadoBadgeClass(string estado) => estado switch
    {
        "Borrador" => "estado-borrador",
        "EnTransito" => "estado-transito",
        "Completado" => "estado-completado",
        "Anulado" => "estado-anulado",
        _ => string.Empty
    };
}
