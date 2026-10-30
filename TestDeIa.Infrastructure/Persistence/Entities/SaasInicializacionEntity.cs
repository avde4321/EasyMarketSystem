namespace TestDeIa.Infrastructure.Persistence.Entities;

// Metadata única de instalación de plataforma, no pertenece a un tenant ni tiene API pública.
public sealed class SaasInicializacionEntity
{
    public string Clave { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
}
