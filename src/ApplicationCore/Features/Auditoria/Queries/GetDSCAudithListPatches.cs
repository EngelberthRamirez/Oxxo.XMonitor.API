using System.Data;
using ApplicationCore.Common.Abstractions.Data;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Auditoria.Queries;

public class GetDSCAudithListPatches
{
    public class Response
    {
        public int PatchId { get; set; }
        public bool IsComponent { get; set; }
        public int ValidationType { get; set; }
        public bool IsPro { get; set; }
        public string ProductVersion { get; set; }
        public string RegistryKey { get; set; }
        public int ReleaseListTypeId { get; set; }
        public int InstallOrder { get; set; }
    }

    public class Query : IRequest<List<Response>>
    {
    }

    public class Handler(IDbConnectionFactory dbConnectionFactory, ILogger<Handler> logger) : IRequestHandler<Query, List<Response>>
    {
        public async Task<List<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            using IDbConnection connection = dbConnectionFactory.CreateConnection("defaultConnection");

            try
            {
                var result = await connection.QueryAsync<Response>(
                    "GetDSCAudithListPatches",
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing stored procedure GetDSCAudithListPatches");
                throw;
            }
        }
    }
}
