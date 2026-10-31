using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Empresa;

public sealed class NumeracionRequest
{
    public Guid PuntoEmisionId { get; set; }
    [RegularExpression("^[12]$")] public string AmbienteSri { get; set; } = "1";
    [RegularExpression("^(01|03|04|07)$")] public string CodigoDocumento { get; set; } = "01";
    [Range(0, 999999998)] public long UltimoSecuencial { get; set; }
    public long ValorEsperado { get; set; }
    [Required, StringLength(500, MinimumLength = 5)] public string Motivo { get; set; } = string.Empty;
}
