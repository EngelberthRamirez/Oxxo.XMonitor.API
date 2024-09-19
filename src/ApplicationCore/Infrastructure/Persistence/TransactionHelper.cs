using System.Data;
using ApplicationCore.Common.Abstractions.Data;

namespace ApplicationCore.Infrastructure.Persistence
{
    public class TransactionHelper : ITransactionHelper
    {
        public async Task<T> ExecuteInTransactionAsync<T>(IDbConnection connection, Func<IDbTransaction, Task<T>> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            using var transaction = connection.BeginTransaction(isolationLevel);

            try
            {
                var result = await action(transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}