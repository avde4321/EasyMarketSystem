namespace TestDeIa.Shared.Responses.Empresa;

public sealed class NumeracionResponse
{
    public Guid PuntoEmisionId { get; set; }
    public string Establecimiento { get; set; } = string.Empty;
    public string PuntoEmision { get; set; } = string.Empty;
    public string AmbienteSri { get; set; } = "1";
    public string CodigoDocumento { get; set; } = "01";
    public long UltimoSecuencial { get; set; }
    public long ProximoSecuencial => UltimoSecuencial + 1;
}

public sealed class NumeracionHistorialResponse
{
    public DateTimeOffset Fecha { get; set; }
    public Guid UsuarioId { get; set; }
    public string Serie { get; set; } = string.Empty;
    public string AmbienteSri { get; set; } = string.Empty;
    public string CodigoDocumento { get; set; } = string.Empty;
    public long Anterior { get; set; }
    public long Nuevo { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
