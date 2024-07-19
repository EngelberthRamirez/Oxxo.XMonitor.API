using System.Data;
using ApplicationCore.Common.Exceptions;
using ApplicationCore.Common.Helpers;
using ApplicationCore.Common.Models;
using ApplicationCore.Infrastructure.Persistence.Context;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ApplicationCore.Features.CashRegister.Commands;

public class RebootCashRegister
{
    public class Request
    {
        public string CRPlaza { get; set; } = default!;
        public string CRTienda { get; set; } = default!;
        public int TipoCaja { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.CRPlaza).NotEmpty();
            RuleFor(r => r.CRTienda).NotEmpty();
            RuleFor(r => r.TipoCaja)
                .NotNull()
                .Must(tipoCaja => tipoCaja == 1 || tipoCaja == 2)
                .WithMessage("TipoCaja no válido. Solo se permiten valores 1 y 2.");
        }
    }

    public class Command : IRequest
    {
        public required Request Parameters { get; set; }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly ApplicationDbContext context;
        private readonly ProxyHelper proxyHelper;
        private readonly ILogger<Handler> logger;
        private readonly ILoggingHelper loggingHelper;

        public Handler(ApplicationDbContext context, ProxyHelper proxyHelper, ILogger<Handler> logger, ILoggingHelper loggingHelper)
        {
            this.context = context;
            this.proxyHelper = proxyHelper;
            this.logger = logger;
            this.loggingHelper = loggingHelper;

            var loggingContext = new LoggingContext
            {
                ConfigurationId = 1,
                Subprocess = "RebootCashRegister",
                UserId = 1,
                UserName = "System"
            };
            loggingHelper.SetContext(loggingContext);
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            loggingHelper.Information($"Starting RebootCashRegisterCommand for CRTienda: {request.Parameters.CRTienda}, CRPlaza: {request.Parameters.CRPlaza}", "Start");

            var requestConfig = await context.RequestConfig.FirstAsync(x => x.Name == "ExecuteCommandWRAsync" && x.MethodName == "ExecuteCommandWRAsync", cancellationToken: cancellationToken)
                                ?? throw new Exception("Request configuration not found.");

            loggingHelper.Information($"Fetched request configuration.", "FetchConfig");

            var storeData = context.StoreDataByUser(userId: 1, active: true)
                .Where(x => x.Code == request.Parameters.CRTienda && x.CRPlaza == request.Parameters.CRPlaza)
                .Where(x => request.Parameters.TipoCaja == 1 ? !x.StoreName.EndsWith("PRO") : x.StoreName.EndsWith("PRO"))
                .ToList();

            if (!storeData.Any())
            {
                var errorMessage = "No se encontraron cajas que coincidan con los parámetros proporcionados.";
                loggingHelper.Error(errorMessage, "Validation");
                throw new NotFoundException(errorMessage);
            }

            var data = JsonConvert.SerializeObject(new List<string> { "shutdown -r -t 0" });
            var parameters = Array.Empty<string>();
            var secureConnection = true;

            foreach (var store in storeData)
            {
                if (requestConfig != null)
                {
                    var httpRequest = new RequestParameters
                    {
                        Id = requestConfig.Guid,
                        CashRegisterIP = store.StoreIp,
                        Parameters = parameters,
                        Data = data,
                        SecureConnection = secureConnection
                    };

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, "proxy/ejecuterequest")
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(httpRequest), System.Text.Encoding.UTF8, "application/json")
                    };
                    var timeout = TimeSpan.FromMinutes(10);

                    try
                    {
                        loggingHelper.Information($"Sending reboot command to Cash Register IP: {store.StoreIp}", "SendCommand");
                        var requestHelper = await proxyHelper.ExecuteRequestAsync(requestMessage, requestConfig.UrlFormat, timeout);
                        loggingHelper.Information($"Reboot command sent successfully to Cash Register IP: {store.StoreIp}", "SendCommandSuccess");
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"RebootCashRegisterCommand Handle ExecuteCommandWRAsync - CRTienda: {request.Parameters.CRTienda}, CRPlaza: {request.Parameters.CRPlaza}";
                        logger.LogError(ex, errorMessage);
                        loggingHelper.Error(ex.Message, "SendCommandFailed");
                    }
                }
            }

            loggingHelper.Information($"Finished RebootCashRegisterCommand for CRTienda: {request.Parameters.CRTienda}, CRPlaza: {request.Parameters.CRPlaza}", "End");
            loggingHelper.Commit();
        }
    }
}
