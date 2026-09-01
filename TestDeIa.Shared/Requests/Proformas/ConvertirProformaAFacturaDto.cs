namespace TestDeIa.Shared.Requests.Proformas;

public sealed class ConvertirProformaAFacturaDto
{
    public Guid ProformaId { get; set; }

    public Guid PuntoEmisionId { get; set; }

    public string FormaPagoId { get; set; } = string.Empty;

    public Guid UsuarioId { get; set; }
}
