using TestDeIa.Domain.Modules.Sri;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SriCatalogoErrorEntity
{
    public int Id { get; set; }

    public string CodigoSri { get; set; } = string.Empty;

    public string MensajeSri { get; set; } = string.Empty;

    public string SolucionSugerida { get; set; } = string.Empty;

    public SriTipoError TipoError { get; set; }
}
