namespace TestDeIa.Application.Common;

public sealed class StockInsuficienteException : InvalidOperationException
{
    public StockInsuficienteException(string message)
        : base(message)
    {
    }
}
