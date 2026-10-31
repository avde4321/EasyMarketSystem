namespace TestDeIa.Shared.Compras;

public static class CompraDocumentTypes
{
    public const string FacturaProveedor = "01";
    public const string NotaVentaRimpe = "02";
    public const string LiquidacionCompra = "03";

    public static bool IsSupported(string? value)
    {
        return value is FacturaProveedor or NotaVentaRimpe or LiquidacionCompra;
    }

    public static string GetName(string? value)
    {
        return value switch
        {
            FacturaProveedor => "Factura",
            NotaVentaRimpe => "Nota de venta",
            LiquidacionCompra => "Liquidacion de compra",
            _ => value?.Trim() ?? string.Empty
        };
    }
}
