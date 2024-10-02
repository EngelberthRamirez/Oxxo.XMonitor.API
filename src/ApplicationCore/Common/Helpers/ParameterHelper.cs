using ApplicationCore.Common.Abstractions.Caching;
using ApplicationCore.Common.Abstractions.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationCore.Common.Helpers
{
    public class ParameterHelper
    {
        private readonly ICacheService _cacheService;
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ITransactionHelper _transactionHelper;

        public ParameterHelper(
            ICacheService cacheService,
            IDbConnectionFactory dbConnectionFactory,
            ITransactionHelper transactionHelper)
        {
            _cacheService = cacheService;
            _dbConnectionFactory = dbConnectionFactory;
            _transactionHelper = transactionHelper;
        }

        public async Task<string> GetByNameAsync(string friendlyName, bool fromCache = true, CancellationToken cancellationToken = default)
        {
            var key = $"par_{friendlyName}";

            if (fromCache)
            {
                return await _cacheService.GetOrCreateAsync(key, async ct => await GetFromDatabaseAsync(friendlyName, ct), cancellationToken: cancellationToken);
            }

            return await GetFromDatabaseAsync(friendlyName, cancellationToken);
        }

        private async Task<string> GetFromDatabaseAsync(string friendlyName, CancellationToken cancellationToken)
        {
            using IDbConnection connection = _dbConnectionFactory.CreateConnection("defaultConnection");
            connection.Open();

            var parameterValue = string.Empty;
            parameterValue = await _transactionHelper.ExecuteInTransactionAsync(connection, async transaction =>
            {
                var getValue = await connection.QueryFirstOrDefaultAsync<string>(
                    "GetParameterByFriendlyName",
                    new { FriendlyName = friendlyName },
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction);

                if (string.IsNullOrEmpty(getValue))
                {
                    return string.Empty;//throw new InvalidOperationException($"Parameter '{friendlyName}' not found or has no value.");
                }

                return getValue;
            });

            return parameterValue;

        }
    }
}
