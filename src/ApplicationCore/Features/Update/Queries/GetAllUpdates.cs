using System.Data;
using System.Text.Json.Serialization;
using ApplicationCore.Common.Abstractions.Data;
using ApplicationCore.Common.Helpers;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Update.Queries
{
    public class GetAllUpdates
    {
        public class ValidateSimultaneosResult
        {
            public int Valid { get; set; }

            public string Msg { get; set; } = string.Empty;
        }

        public class GetAvailableUpdatesResult
        {
            public int TaskId;

            public bool? IsChained;

            public int StoreDataId;

            public string StoreIp { get; set; } = string.Empty;

            public bool HasSlowInternet;

            public int ReleaseTaskTypeId;

            public bool? HighPriority;

            public int? ExpirationDays;
        }

        public class Response
        {

            [JsonPropertyName("i")]
            public int Id { get; set; }

            [JsonPropertyName("ic")]
            public bool? IsChained { get; set; }

            [JsonIgnore]
            public int StoreDataId { get; set; }

            [JsonPropertyName("pv")]
            public string Pivot { get; set; } = string.Empty;

            [JsonPropertyName("rtt")]
            public int ReleaseTaskTypeId { get; set; }

            [JsonPropertyName("dwa")]
            public List<string> DownloadAgents { get; set; } = new List<string>();

            [JsonPropertyName("exc")]
            public bool Execute { get; set; }

            [JsonPropertyName("excr")]
            public string Reason { get; set; } = string.Empty;

            [JsonPropertyName("ac")]
            public string AzureCredencial { get; set; } = string.Empty;

            [JsonPropertyName("Sp")]
            public string SizePart { get; set; } = string.Empty;

            [JsonPropertyName("hp")]
            public bool? HighPriority { get; set; }

            [JsonPropertyName("ed")]
            public int? ExpirationDays { get; set; }

            [JsonPropertyName("stp")]
            public List<Step> Steps { get; set; } = [];
        }

        public class Step
        {
            [JsonPropertyName("i")]
            public int Id { get; set; }

            [JsonPropertyName("st")]
            public int StepType { get; set; }
        }

        public class Query : IRequest<List<Response>>
        {
            public string StoreName { get; set; } = string.Empty;
            public int StoreDataId { get; set; }
            public int TaskId { get; set; }
            public int StoreId { get; set; }
        }


        public class Handler(IDbConnectionFactory dbConnectionFactory, ITransactionHelper transactionHelper, ILogger<Handler> logger, ParameterHelper parameterHelper) : IRequestHandler<Query, List<Response>>
        {
            public async Task<List<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    using IDbConnection connection = dbConnectionFactory.CreateConnection("defaultConnection");
                    connection.Open();

                    var availableUpdates = await transactionHelper.ExecuteInTransactionAsync(connection, async transaction =>
                    {
                        var updates = await GetUpdates(request, connection, transaction);
                        return updates ?? [];
                    }, IsolationLevel.ReadUncommitted);

                    if (!availableUpdates.Any())
                    {
                        return [];
                    }

                    // Obtener varios parámetros relacionados con la actualización de otras tablas o configuraciones
                    var satAgents = (await parameterHelper.GetByNameAsync("SatelliteAgents", true, cancellationToken)).Split('|').ToList();
                    var agents = (await parameterHelper.GetByNameAsync("DownloadAgents", true, cancellationToken)).Split('|').ToList();
                    var azureCredentials = await parameterHelper.GetByNameAsync("StorageConnString", true, cancellationToken);
                    var sizeParts = await parameterHelper.GetByNameAsync("SizePart", true, cancellationToken);

                    var tasks = availableUpdates.Select(n => new Response
                    {
                        Id = n.TaskId,
                        StoreDataId = n.StoreDataId,
                        IsChained = n.IsChained,
                        HighPriority = n.HighPriority,
                        ExpirationDays = n.ExpirationDays,
                        Pivot = n.StoreIp,
                        Execute = true,
                        DownloadAgents = n.HasSlowInternet ? satAgents : agents,
                        ReleaseTaskTypeId = n.ReleaseTaskTypeId,
                        AzureCredencial = azureCredentials,
                        SizePart = sizeParts
                    }).ToList();

                    foreach (var task in tasks)
                    {
                        var buffer = await transactionHelper.ExecuteInTransactionAsync(connection, async transaction =>
                        {
                            request.TaskId = task.Id;
                            request.StoreId = task.StoreDataId;
                            return await ValidateSimultaneos(request, connection, transaction);
                        }, IsolationLevel.ReadUncommitted);

                        var validateResult = buffer.First();
                        if (validateResult.Valid == 0)
                        {
                            task.Execute = false;
                            task.Reason = validateResult.Msg;
                            task.Steps = [new Step { Id = 0, StepType = 0 }];
                        }

                    }

                    return tasks;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in GetAvailableUpdates Handler");
                    throw;
                }
            }

            private static async Task<IEnumerable<GetAvailableUpdatesResult>> GetUpdates(Query request,
                IDbConnection connection, IDbTransaction transaction)
            {
                return await connection.QueryAsync<GetAvailableUpdatesResult>(
                    "dbo.GetAvailableUpdates",
                    new
                    {
                        request.StoreName,
                        Type = 1,
                        TaskId = 0,
                        request.StoreDataId,
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction
                );
            }

            private static async Task<IEnumerable<ValidateSimultaneosResult>> ValidateSimultaneos(Query request,
                IDbConnection connection, IDbTransaction transaction)
            {
                return await connection.QueryAsync<ValidateSimultaneosResult>(
                    "dbo.FnValidateSimultaneos",
                    new
                    {
                        request.TaskId,
                        request.StoreId,
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction
                );
            }

        }
    }
}
