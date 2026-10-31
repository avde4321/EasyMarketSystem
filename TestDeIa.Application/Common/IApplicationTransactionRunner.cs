namespace TestDeIa.Application.Common;

public interface IApplicationTransactionRunner
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}
