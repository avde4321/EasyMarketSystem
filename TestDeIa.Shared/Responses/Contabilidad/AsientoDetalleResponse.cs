namespace TestDeIa.Shared.Responses.Contabilidad;

public sealed class AsientoDetalleResponse
{
    public Guid Id { get; set; }
    public Guid CuentaContableId { get; set; }
    public string CuentaCodigo { get; set; } = string.Empty;
    public string CuentaNombre { get; set; } = string.Empty;
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}
