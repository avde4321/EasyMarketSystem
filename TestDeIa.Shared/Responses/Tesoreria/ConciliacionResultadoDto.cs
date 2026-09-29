namespace TestDeIa.Shared.Responses.Tesoreria;

public sealed class ConciliacionResultadoDto
{
    public Guid CuentaBancariaId { get; set; }
    public int TotalExtractosEvaluados { get; set; }
    public int TotalConciliados { get; set; }
    public int TotalSugerencias { get; set; }
    public decimal SaldoConciliado { get; set; }
    public IReadOnlyCollection<ConciliacionMatchDto> Coincidencias { get; set; } = Array.Empty<ConciliacionMatchDto>();
}
