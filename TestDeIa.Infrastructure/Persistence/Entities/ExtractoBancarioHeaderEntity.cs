namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ExtractoBancarioHeaderEntity
{
    public Guid Id { get; set; }
    public Guid CuentaBancariaId { get; set; }
    public DateTime FechaImportacion { get; set; }
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public string NombreArchivoOriginal { get; set; } = string.Empty;
    public int TotalRegistros { get; set; }
    public string? Observaciones { get; set; }

    public CuentaBancariaEntity CuentaBancaria { get; set; } = default!;
    public ICollection<ExtractoBancarioDetalleEntity> Detalles { get; set; } = [];
}
