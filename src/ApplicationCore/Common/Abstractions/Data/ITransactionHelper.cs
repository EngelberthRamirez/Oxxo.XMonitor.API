using System.Data;

namespace ApplicationCore.Common.Abstractions.Data
{
    public interface ITransactionHelper
    {
        Task<T> ExecuteInTransactionAsync<T>(IDbConnection connection, Func<IDbTransaction, Task<T>> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}
