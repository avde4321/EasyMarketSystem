namespace TestDeIa.Application.Common;

public sealed class ValidadorEcuadorService
{
    public bool EsCedulaValida(string? cedula) => TestDeIa.Shared.Validation.ValidadorEcuador.EsCedulaValida(cedula);

    public bool EsRucPersonaJuridicaValido(string? ruc) => TestDeIa.Shared.Validation.ValidadorEcuador.EsRucPersonaJuridicaValido(ruc);

    public bool EsRucNaturalValido(string? ruc) => TestDeIa.Shared.Validation.ValidadorEcuador.EsRucNaturalValido(ruc);
}
