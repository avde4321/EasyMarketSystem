namespace TestDeIa.Domain.Modules.Financiero.Entities;

public sealed class ConsolidadoIvaTarifa
{
    public decimal Base0 { get; init; }
    public decimal Base5 { get; init; }
    public decimal Base8 { get; init; }
    public decimal Base15 { get; init; }
    public decimal Iva5 { get; init; }
    public decimal Iva8 { get; init; }
    public decimal Iva15 { get; init; }

    public decimal BaseTarifaDiferenteCero => Base5 + Base8 + Base15;
    public decimal IvaTarifaDiferenteCero => Iva5 + Iva8 + Iva15;
}
