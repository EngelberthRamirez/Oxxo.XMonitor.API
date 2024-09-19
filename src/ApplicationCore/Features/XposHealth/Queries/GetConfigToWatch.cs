using System.Data;
using ApplicationCore.Common.Abstractions.Data;
using ApplicationCore.Common.Abstractions.Messaging;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.XposHealth.Queries;

public class GetConfigToWatch
{
    public class Response
    {
        public List<FilesToWatch> Files { get; set; } = [];
        public IEnumerable<EventsLogToWatch> Events { get; set; } = [];
    }

    public class FilesToWatch
    {
        public string File { get; set; } = default!;
        public string Path { get; set; } = default!;
        public string Name { get; set; } = default!;
        public bool? AddToWatch { get; set; }
        public int ItemType { get; set; }
        public string FullName => $"{Path}{File}";
    }

    public class EventsLogToWatch
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string LogName { get; set; } = default!;
        public int EntryType { get; set; }
        public int? Online { get; set; }
    }

    public class Query : ICachedQuery<Response>
    {
        public string Key => "ConfigToWatch";

        public TimeSpan? Expiration => null;
    }

    public class Handler(IDbConnectionFactory dbConnectionFactory, ITransactionHelper transactionHelper, ILogger<Handler> logger) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                using IDbConnection connection = dbConnectionFactory.CreateConnection("defaultConnection");
                connection.Open();

                return await transactionHelper.ExecuteInTransactionAsync(connection, async transaction =>
                {
                    var files = await GetFilesToWatchAsync(connection, transaction);
                    var events = await GetEventsLogToWatchAsync(connection, transaction);

                    return new Response
                    {
                        Files = files.ToList(),
                        Events = events.ToList()
                    };
                }, IsolationLevel.ReadUncommitted);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetConfigToWatch Handler");
                throw;
            }
        }

        private static async Task<IEnumerable<FilesToWatch>> GetFilesToWatchAsync(IDbConnection connection, IDbTransaction transaction)
        {
            return await connection.QueryAsync<FilesToWatch>(
                @"SELECT cd.FileName AS 'File', cd.Path, cd.Name, cd.AddToWatch, cd.DynamicCategory AS ItemType
                  FROM Config s
                  JOIN ConfigDetail cd ON s.Id = cd.ConfigId
                  WHERE s.Descripcion = 'Default' AND cd.AddToWatch = 1 AND cd.DynamicCategory != 4",
                transaction: transaction
            );
        }

        private static async Task<IEnumerable<EventsLogToWatch>> GetEventsLogToWatchAsync(IDbConnection connection, IDbTransaction transaction)
        {
            return await connection.QueryAsync<EventsLogToWatch>(
                @"SELECT n.Id, n.EventId, n.LogName, n.EntryType, n.Online 
                  FROM EventsToWatchConfig n",
                transaction: transaction
            );
        }
    }
}
