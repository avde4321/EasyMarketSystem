namespace TestDeIa.Domain.Modules.Inventario;

public static class TipoMovimientoInventario
{
    public const string EntradaCompra = "ENTRADA_COMPRA";
    public const string SalidaVenta = "SALIDA_VENTA";
    public const string DevolucionVenta = "DEVOLUCION_VENTA";
    public const string AjusteIngreso = "AJUSTE_INGRESO";
    public const string AjusteEgreso = "AJUSTE_EGRESO";
    public const string TransferenciaEntrada = "TRANSFERENCIA_ENTRADA";
    public const string TransferenciaSalida = "TRANSFERENCIA_SALIDA";
    public const string MermaInventario = "MERMA_INVENTARIO";
    public const string TomaFisica = "TOMA_FISICA";

    public static readonly IReadOnlySet<string> Entradas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        EntradaCompra,
        DevolucionVenta,
        AjusteIngreso,
        TransferenciaEntrada
    };

    public static readonly IReadOnlySet<string> Salidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        SalidaVenta,
        AjusteEgreso,
        TransferenciaSalida,
        MermaInventario
    };

    public static readonly IReadOnlySet<string> Todos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        EntradaCompra,
        SalidaVenta,
        DevolucionVenta,
        AjusteIngreso,
        AjusteEgreso,
        TransferenciaEntrada,
        TransferenciaSalida,
        MermaInventario,
        TomaFisica
    };

    public static string Normalize(string tipoMovimiento)
    {
        var normalized = tipoMovimiento.Trim().ToUpperInvariant();

        return normalized switch
        {
            "ENTRADA" or "INGRESO_COMPRA" => EntradaCompra,
            "SALIDA" or "EGRESO_FACTURA" => SalidaVenta,
            "DEVOLUCION" or "DEVOLUCION_VENTA" => DevolucionVenta,
            "INGRESO_AJUSTE" => AjusteIngreso,
            "EGRESO_AJUSTE" => AjusteEgreso,
            "EGRESO_MERMA" => MermaInventario,
            "TRANSFERENCIA_DESPACHO" => TransferenciaSalida,
            "TRANSFERENCIA_RECEPCION" => TransferenciaEntrada,
            _ => normalized
        };
    }

    public static bool EsEntrada(string tipoMovimiento) => Entradas.Contains(Normalize(tipoMovimiento));

    public static bool EsSalida(string tipoMovimiento) => Salidas.Contains(Normalize(tipoMovimiento));
}
