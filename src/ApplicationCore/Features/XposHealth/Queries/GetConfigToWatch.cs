using System.Data;
using ApplicationCore.Common.Abstractions.Data;
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

    public class Query : IRequest<Response>
    {
    }

    public class Handler(IDbConnectionFactory dbConnectionFactory, ILogger<Handler> logger) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            using IDbConnection connection = dbConnectionFactory.CreateConnection("defaultConnection");

            try
            {
                var files = await connection.QueryAsync<FilesToWatch>(
                    @"SELECT cd.FileName AS 'File', cd.Path, cd.Name, cd.AddToWatch, cd.DynamicCategory AS ItemType
                      FROM Config s
                      JOIN ConfigDetail cd ON s.Id = cd.ConfigId
                      WHERE s.Descripcion = 'Default' AND cd.AddToWatch = 1 AND cd.DynamicCategory != 4"
                );

                var events = await connection.QueryAsync<EventsLogToWatch>(
                    @"SELECT n.Id, n.EventId, n.LogName, n.EntryType, n.Online 
                      FROM EventsToWatchConfig n"
                );

                var configToWatch = new Response
                {
                    Files = files.ToList(),
                    Events = events.ToList()
                };

                return configToWatch;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving config to watch");
                throw;
            }
        }
    }
}
