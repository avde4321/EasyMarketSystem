namespace TestDeIa.Domain.Modules.Financiero.Entities;

public sealed class MemoriaAnalisisFiscal
{
    public Guid Id { get; init; }
    public Guid EmpresaId { get; init; }
    public int Mes { get; init; }
    public int Anio { get; init; }
    public string ResumenNumericoJson { get; init; } = string.Empty;
    public string RazonamientoIA { get; init; } = string.Empty;
    public string ContextoPrevioUtilizado { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public Guid UsuarioCreacionId { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public Guid? UsuarioModificacionId { get; init; }
}
