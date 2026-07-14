namespace TestDeIa.Domain.Modules.Inventario.Entities;

public sealed class Producto
{
    public Producto(
        Guid id,
        string codigo,
        string nombre,
        string? descripcion,
        string codigoIva,
        decimal porcentajeIva,
        decimal precioVenta,
        decimal stockActual,
        decimal? stockMinimo,
        decimal costoPromedio,
        bool controlaStock,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        CodigoIva = codigoIva;
        PorcentajeIva = porcentajeIva;
        PrecioVenta = precioVenta;
        StockActual = stockActual;
        StockMinimo = stockMinimo;
        CostoPromedio = costoPromedio;
        ControlaStock = controlaStock;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }

    public string Codigo { get; }

    public string Nombre { get; }

    public string? Descripcion { get; }

    public string CodigoIva { get; }

    public decimal PorcentajeIva { get; }

    public decimal PrecioVenta { get; }

    public decimal StockActual { get; }

    public decimal? StockMinimo { get; }

    public decimal CostoPromedio { get; }

    public bool ControlaStock { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; }
}

