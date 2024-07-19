using System.Data;
using ApplicationCore.Common.Abstractions.Data;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Auditoria.Queries;

public class GetDscFullConfig
{
    public class Response
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Query : IRequest<List<Response>>
    {
    }

    public class Handler(IDbConnectionFactory dbConnectionFactory, ILogger<Handler> logger) : IRequestHandler<Query, List<Response>>
    {
        public async Task<List<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            const string sql = "SELECT Id, Name, Value FROM DscGlobalParameters";

            using IDbConnection connection = dbConnectionFactory.CreateConnection("defaultConnection");

            try
            {
                var result = await connection.QueryAsync<Response>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving configuration from DscGlobalParameters");
                throw;
            }
        }
    }
}
