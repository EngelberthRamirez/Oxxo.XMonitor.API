using ApplicationCore.Common.Helpers;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using WebApi.Filters;

namespace WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "My API",
                Version = "v1"
            });
            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please insert JWT with Bearer into field",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });

            c.CustomSchemaIds(type => type.FullName.Replace("+", "."));
        });

        services.AddControllers(options =>
            options.Filters.Add<ApiExceptionFilterAttribute>());

        services.AddHttpClient<ProxyHelper>(client =>
        {
            client.BaseAddress = new Uri(configuration["Environments:ProxyHub"]!);
        });

        services.AddHttpClient<ILoggingHelper, LoggingHelper>(client =>
        {
            //client.BaseAddress = new Uri(configuration["LoggingOptions:ServiceUrl"]!);
        });
        services.Configure<LoggingOptions>(configuration.GetSection("LoggingOptions"));

        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        return services;
    }
}