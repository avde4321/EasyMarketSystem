using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Inventario;

public sealed class ProductoRequest
{
    [Required(ErrorMessage = "El codigo es obligatorio.")]
    [StringLength(40, ErrorMessage = "El codigo no puede superar 40 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(160, ErrorMessage = "El nombre no puede superar 160 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripcion no puede superar 300 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El codigo fiscal de IVA es obligatorio.")]
    [StringLength(20, ErrorMessage = "El codigo fiscal de IVA no puede superar 20 caracteres.")]
    public string CodigoIva { get; set; } = "IVA_15";

    [Range(0, 100, ErrorMessage = "El porcentaje de IVA debe estar entre 0 y 100.")]
    public decimal PorcentajeIva { get; set; } = 15;

    [Range(0, 999999999, ErrorMessage = "El precio de venta no puede ser negativo.")]
    public decimal PrecioVenta { get; set; }

    [Range(0, 999999999, ErrorMessage = "El stock minimo no puede ser negativo.")]
    public decimal? StockMinimo { get; set; }

    [Range(0, 999999999, ErrorMessage = "El costo inicial no puede ser negativo.")]
    public decimal CostoInicial { get; set; }

    [Range(0, 999999999, ErrorMessage = "El stock inicial no puede ser negativo.")]
    public decimal StockInicial { get; set; }

    public bool ControlaStock { get; set; } = true;

    public bool IsActive { get; set; } = true;
}

