namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SriEstadoComprobanteEntity
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public bool RequiereReenvioRecepcion { get; set; }

    public bool RequiereConsultaAutorizacion { get; set; }

    public bool EsEstadoFinal { get; set; }

    public bool EsEditable { get; set; }
}
