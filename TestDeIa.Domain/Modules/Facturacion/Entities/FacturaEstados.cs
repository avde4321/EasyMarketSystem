namespace TestDeIa.Domain.Modules.Facturacion.Entities;

public enum FacturaEstado
{
    NO_FIRMADO = 1,
    PENDIENTE = 2,
    AUTORIZADO = 3,
    RECHAZADO = 4
}

public static class FacturaEstadoExtensions
{
    public static string ToApiValue(this FacturaEstado estado)
    {
        return estado.ToString();
    }

    public static bool IsFinal(this FacturaEstado estado)
    {
        return estado is FacturaEstado.AUTORIZADO or FacturaEstado.RECHAZADO;
    }
}
