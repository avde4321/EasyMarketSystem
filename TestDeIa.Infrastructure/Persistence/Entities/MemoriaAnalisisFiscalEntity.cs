namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class MemoriaAnalisisFiscalEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public string ResumenNumericoJson { get; set; } = string.Empty;
    public string RazonamientoIA { get; set; } = string.Empty;
    public string ContextoPrevioUtilizado { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UsuarioCreacionId { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UsuarioModificacionId { get; set; }
}
