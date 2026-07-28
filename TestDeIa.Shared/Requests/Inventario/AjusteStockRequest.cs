using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class AjusteStockRequest
{
    public Guid? BodegaId { get; set; }

    [Required(ErrorMessage = "El tipo de movimiento es obligatorio.")]
    public string TipoMovimiento { get; set; } = "Entrada";

    [Required(ErrorMessage = "El concepto es obligatorio.")]
    [StringLength(160, ErrorMessage = "El concepto no puede superar 160 caracteres.")]
    public string Concepto { get; set; } = "Ajuste manual";

    [StringLength(80, ErrorMessage = "La referencia no puede superar 80 caracteres.")]
    public string? Referencia { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public decimal Cantidad { get; set; }

    [Range(0, 999999999, ErrorMessage = "El costo unitario no puede ser negativo.")]
    public decimal CostoUnitario { get; set; }
}
