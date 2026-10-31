using TestDeIa.Domain.Modules.Compras.Entities;

namespace TestDeIa.Application.Modules.Compras.Models;

public sealed class EstudioMercadoAnalisisResult
{
    public IReadOnlyCollection<EstudioMercadoSugerenciaCompra> SugerenciasCompra { get; init; } = Array.Empty<EstudioMercadoSugerenciaCompra>();
    public string AnalisisEstrategicoIA { get; init; } = string.Empty;
}
