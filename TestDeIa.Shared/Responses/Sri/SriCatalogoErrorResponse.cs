namespace TestDeIa.Shared.Responses.Sri;

public sealed class SriCatalogoErrorResponse
{
    public int Id { get; set; }

    public string CodigoSri { get; set; } = string.Empty;

    public string MensajeSri { get; set; } = string.Empty;

    public string SolucionSugerida { get; set; } = string.Empty;

    public string TipoError { get; set; } = string.Empty;
}
