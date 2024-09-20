using System.Data;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ApplicationCore.Common.Abstractions.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationCore.Features.Update.Queries
{
    public class GetAllUpdates
    {
        public class Response
        {
            public int TaskId { get; set; }
            public int StoreDataId { get; set; }
            public bool IsChained { get; set; }
            public string StoreIp { get; set; }
            public bool HasSlowInternet { get; set; }
            public string ThisIp { get; set; }
            public int ReleaseTaskTypeId { get; set; }
            public bool HighPriority { get; set; }
            public int ExpirationDays { get; set; }
        }

        public class Query : IRequest<List<Response>>
        {
            public string StoreName { get; set; }
            public int StoreDataId { get; set; }
        }

        public class Handler : IRequestHandler<Query, List<Response>>
        {
            private readonly IDbConnectionFactory _dbConnectionFactory;
            private readonly ILogger<Handler> _logger;

            public Handler(IDbConnectionFactory dbConnectionFactory, ILogger<Handler> logger)
            {
                _dbConnectionFactory = dbConnectionFactory;
                _logger = logger;
            }

            public async Task<List<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                using IDbConnection connection = _dbConnectionFactory.CreateConnection("defaultConnection");

                try
                {
                    var result = await connection.QueryAsync<Response>(
                        "GetAvailableUpdates",
                        new { 
                            StoreName = request.StoreName,
                            Type = 1,
                            TaskId = 0,
                            StoreDataId = request.StoreDataId,
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    return result.ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing stored procedure GetAvailableUpdates");
                    throw;
                }
            }

        }
    }
}
