using System.Data;
using ApplicationCore.Common.Abstractions.Data;
using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Auditoria.Queries;

public class GetLastAudithByStoreName
{
    public class Request
    {
        public string StoreName { get; set; } = default!;
    }

    public class Query : IRequest<int>
    {
        public required Request Request { get; set; }
    }

    public class Handler(IDbConnectionFactory dbConnectionFactory, ILogger<Handler> logger) : IRequestHandler<Query, int>
    {
        public async Task<int> Handle(Query query, CancellationToken cancellationToken)
        {
            using IDbConnection connection = dbConnectionFactory.CreateConnection("defaultConnection");

            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<int>(
                    "DscGetLastAudithByStoreName",
                    new { query.Request.StoreName },
                    commandType: CommandType.StoredProcedure
                );
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing stored procedure DscGetLastAudithByStoreName with StoreName: {StoreName}", query.Request.StoreName);
                throw;
            }
        }
    }
}
