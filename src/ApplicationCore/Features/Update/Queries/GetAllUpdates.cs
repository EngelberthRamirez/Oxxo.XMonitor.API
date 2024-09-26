using System.Data;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ApplicationCore.Common.Abstractions.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Common.Helpers;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using static ApplicationCore.Features.XposHealth.Queries.GetConfigToWatch;
using ApplicationCore.Infrastructure.Persistence;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Logging;
using System.Security.Cryptography.X509Certificates;

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

            public System.Nullable<bool> IsChained;

            public int StoreDataId;

            public string StoreIp { get; set; } = string.Empty;

            public bool HasSlowInternet;

            public int ReleaseTaskTypeId;

            public System.Nullable<bool> HighPriority;

            public System.Nullable<int> ExpirationDays;
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
            public List<Step> Steps { get; set; } = new List<Step>();
        }

        public class Step
        {
            //#region Fields

            //private bool? _showUi;

            //#endregion

            //#region Properties

            [JsonPropertyName("i")]
            public int Id { get; set; }

            //[JsonPropertyName("ti")]
            //public int TaskId { get; set; }

            //[JsonPropertyName("d")]
            //public string Descripcion { get; set; }

            [JsonPropertyName("st")]
            public int StepType { get; set; }

            //[JsonPropertyName("df")]
            //public DownloadFile DownloadFile { get; set; }

            //[JsonPropertyName("c")]
            //public string Command { get; set; }

            //[JsonPropertyName("ct")]
            //public int? CommandType { get; set; }

            //[JsonPropertyName("cs")]
            //public string CheckSum { get; set; }

            //[JsonPropertyName("p")]
            //public string Path { get; set; }

            //[JsonPropertyName("ip")]
            //public bool? IsPro { get; set; }

            //[JsonPropertyName("ic")]
            //public bool? IsChained { get; set; }

            //[JsonPropertyName("ra")]
            //public bool? RunAsAdmin { get; set; }

            //[JsonPropertyName("ui")]
            //public bool? ShowUI
            //{
            //    get
            //    {
            //        if (RunAsAdmin.HasValue && _showUi == true)
            //        {
            //            return _showUi;
            //        }

            //        return false;
            //    }
            //    set => _showUi = value;
            //}

            //[JsonPropertyName("pi")]
            //public string ProcessToImp { get; set; }

            //public string FullPath => String.Concat(Path + "\\", DownloadFile?.Name ?? String.Empty);

            //[JsonPropertyName("pr")]
            //public string Cmdparameters { get; set; }

            //[JsonPropertyName("pl")]
            //public string Pathlog { get; set; }

            //[JsonPropertyName("sl")]
            //public bool Savelog { get; set; }

            //[JsonPropertyName("schd")]
            //public string SchDate { get; set; }

            //[JsonPropertyName("scht")]
            //public string SchTime { get; set; }

            //[JsonPropertyName("fn")]
            //public string FileName { get; set; }

            //[IgnoreDataMember]
            //public bool HasDownloadFile => new[] { 2, 4 }.Contains(StepType) && DownloadFile != null;

            //[JsonPropertyName("tn")]
            //public int MaxThread { get; set; }

            //[JsonPropertyName("db")]
            //public string DBApply { get; set; }

            //[JsonPropertyName("dbu")]
            //public string DBUser { get; set; }

            //[JsonPropertyName("dbp")]
            //public string Password { get; set; }

            //[JsonPropertyName("met")]
            //public int MaximumExecutionTime { get; set; }

            //[JsonPropertyName("mnr")]
            //public int MaximumReturnRows { get; set; }

            //[JsonPropertyName("order")]
            //public int Order { get; set; }

            //[JsonPropertyName("ex")]
            //public bool? Execute { get; set; }

            //[JsonPropertyName("prms")]
            //public System.Collections.Generic.List<string> Params { get; set; }

            //#endregion
        }

        public class Query : IRequest<List<Response>>
        {
            public string StoreName { get; set; } = string.Empty;
            public int StoreDataId { get; set; }
            public int TaskId { get; set; }
            public int StoreId { get; set; }
        }


        public class Handler : IRequestHandler<Query, List<Response>>
        {
            private readonly IDbConnectionFactory _dbConnectionFactory;
            private readonly ITransactionHelper _transactionHelper;
            private readonly ILogger<Handler> _logger;
            private readonly ParameterHelper _parameterHelper;

            public Handler(IDbConnectionFactory dbConnectionFactory, ITransactionHelper transactionHelper, ILogger<Handler> logger, ParameterHelper parameterHelper)
            {
                _dbConnectionFactory = dbConnectionFactory;
                _transactionHelper = transactionHelper;
                _logger = logger;
                _parameterHelper = parameterHelper;
            }

            public async Task<List<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    using IDbConnection connection = _dbConnectionFactory.CreateConnection("defaultConnection");
                    connection.Open();

                    var availableUpdates = await _transactionHelper.ExecuteInTransactionAsync(connection, async transaction =>
                    {
                        var updates = await GetUpdates(request, connection, transaction);
                        return updates ?? new List<GetAvailableUpdatesResult>();
                    }, IsolationLevel.ReadUncommitted);

                    if (!availableUpdates.Any())
                    {
                        return new List<Response>();
                    }
                    
                    // Obtener varios parámetros relacionados con la actualización de otras tablas o configuraciones
                    var satAgents = (await _parameterHelper.GetByNameAsync("SatelliteAgents", true, cancellationToken)).Split('|').ToList();
                    var agents = (await _parameterHelper.GetByNameAsync("DownloadAgents", true, cancellationToken)).Split('|').ToList();
                    var azureCredentials = await _parameterHelper.GetByNameAsync("StorageConnString", true, cancellationToken);
                    var sizeParts = await _parameterHelper.GetByNameAsync("SizePart", true, cancellationToken);

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
                        var buffer = await _transactionHelper.ExecuteInTransactionAsync(connection, async transaction =>
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
                            task.Steps = new List<Step> { new Step { Id = 0, StepType = 0 } };
                        }

                    }

                    return tasks;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetAvailableUpdates Handler");
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
                        StoreName = request.StoreName,
                        Type = 1,
                        TaskId = 0,
                        StoreDataId = request.StoreDataId,
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
                        TaskId = request.TaskId,
                        StoreId = request.StoreId,
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction
                );
            }

        }
    }
}
