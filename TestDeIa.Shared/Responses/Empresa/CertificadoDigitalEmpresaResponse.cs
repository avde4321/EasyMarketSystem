namespace TestDeIa.Shared.Responses.Empresa;

public sealed class CertificadoDigitalEmpresaResponse
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string EmpresaNombre { get; set; } = string.Empty;

    public string EmpresaRuc { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string NombreArchivo { get; set; } = string.Empty;

    public string? Sujeto { get; set; }

    public string? Emisor { get; set; }

    public string? NumeroSerie { get; set; }

    public string? HuellaDigital { get; set; }

    public DateTimeOffset FechaInicioVigencia { get; set; }

    public DateTimeOffset FechaFinVigencia { get; set; }

    public int DiasParaCaducar { get; set; }

    public bool EstaVigente { get; set; }

    public bool EstaCaducado { get; set; }

    public bool RequiereAlerta { get; set; }

    public string EstadoVigencia { get; set; } = string.Empty;

    public string NivelAlerta { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool EsPrincipal { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
