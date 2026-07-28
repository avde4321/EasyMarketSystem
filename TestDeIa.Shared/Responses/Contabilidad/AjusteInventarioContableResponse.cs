namespace TestDeIa.Shared.Responses.Contabilidad;

public sealed class AjusteInventarioContableResponse
{
    public decimal ValorKardex { get; set; }
    public decimal SaldoCuentaInventario { get; set; }
    public decimal Diferencia { get; set; }
    public bool AsientoGenerado { get; set; }
    public string? NumeroAsiento { get; set; }
}
