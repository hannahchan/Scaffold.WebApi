namespace Scaffold.WebApi.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;
    using Polly;
    using Scaffold.WebApi.Controllers;
    using Scaffold.WebApi.HttpMessageHandlers;
    using Scaffold.WebApi.Middleware;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public static class ServiceCollectionExtensions
    {
        private static readonly string[] IgnorePatterns =
        {
            "^/health$",
            "^/metrics$",
        };

        public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSwaggerGen(options =>
            {
                OpenApiInfo info = new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Scaffold",
                    Description = "A simple CRUD application to demonstrate Scaffold.",
                };

                options.SwaggerDoc("v1", info);

                options.CustomOperationIds(apiDescription =>
                    apiDescription.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null);

                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            return services;
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddTransient<RequestLoggingHttpMessageHandler>();

            services.AddHttpClient<TracingDemoController.IClient, TracingDemoController.Client>()
                .AddTransientHttpErrorPolicy(builder => builder.RetryAsync(10))
                .AddHttpMessageHandler<RequestLoggingHttpMessageHandler>();

            return services;
        }

        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration config)
        {
            string serviceName = config.GetValue("OpenTelemetry:ServiceName", "Scaffold");
            string serviceNamespace = config.GetValue("OpenTelemetry:ServiceNamespace", string.Empty);

            ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceNamespace);

            services.AddOpenTelemetryTracing(builder => builder
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = httpContext => !httpContext.Request.Path.IgnorePath(IgnorePatterns);
                })
                .AddHttpClientInstrumentation()
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = config.GetValue("Jaeger:AgentHost", "localhost");
                    options.AgentPort = config.GetValue("Jaeger:AgentPort", 6831);
                }));

            return services;
        }

        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            services
                .Configure<RequestLoggingMiddleware.Options>(options => options.IgnorePatterns = IgnorePatterns);

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddAutoMapper(typeof(Startup).Assembly);

            return services;
        }

        private static bool IgnorePath(this PathString path, IEnumerable<string> ignorePatterns)
        {
            foreach (string ignorePattern in ignorePatterns)
            {
                if (Regex.IsMatch(path, ignorePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
