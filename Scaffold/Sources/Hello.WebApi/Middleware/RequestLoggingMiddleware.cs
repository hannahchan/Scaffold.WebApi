namespace Scaffold.WebApi.Middleware
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class RequestLoggingMiddleware
    {
        private const string RequestStartedMessageTemplate = "Inbound HTTP {HttpMethod} {RequestPath} started";

        private const string RequestFinishedMessageTemplate = "Inbound HTTP {HttpMethod} {RequestPath} finished - {StatusCode}";

        private static readonly Action<ILogger, string, PathString, Exception?> LogRequestStarted =
            LoggerMessage.Define<string, PathString>(LogLevel.Information, default, RequestStartedMessageTemplate);

        private static readonly Action<ILogger, string, PathString, int, Exception?> LogRequestFinishedInformation =
            LoggerMessage.Define<string, PathString, int>(LogLevel.Information, default, RequestFinishedMessageTemplate);

        private static readonly Action<ILogger, string, PathString, int, Exception?> LogRequestFinishedWarning =
            LoggerMessage.Define<string, PathString, int>(LogLevel.Warning, default, RequestFinishedMessageTemplate);

        private static readonly Action<ILogger, string, PathString, int, Exception?> LogRequestFinishedError =
            LoggerMessage.Define<string, PathString, int>(LogLevel.Error, default, RequestFinishedMessageTemplate);

        private static readonly Action<ILogger, string, PathString, string, Exception?> LogRequestFinishedCritical =
            LoggerMessage.Define<string, PathString, string>(LogLevel.Critical, default, RequestFinishedMessageTemplate);

        private readonly RequestDelegate next;

        private readonly ILogger logger;

        private readonly Options options;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, IOptions<Options> options)
        {
            this.next = next;
            this.logger = logger;
            this.options = options.Value;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            HttpRequest request = httpContext.Request;
            HttpResponse response = httpContext.Response;

            if (IgnorePath(request.Path, this.options))
            {
                await this.next.Invoke(httpContext);
                return;
            }

            LogRequestStarted(this.logger, request.Method, request.Path, null);

            try
            {
                await this.next.Invoke(httpContext);

                if (response.StatusCode >= 200 && response.StatusCode <= 299)
                {
                    LogRequestFinishedInformation(this.logger, request.Method, request.Path, response.StatusCode, null);
                    return;
                }

                if (response.StatusCode >= 500)
                {
                    LogRequestFinishedError(this.logger, request.Method, request.Path, response.StatusCode, null);
                    return;
                }

                LogRequestFinishedWarning(this.logger, request.Method, request.Path, response.StatusCode, null);
            }
            catch (Exception exception)
            {
                LogRequestFinishedCritical(this.logger, request.Method, request.Path, "Unhandled Exception", exception);
                throw;
            }
        }

        private static bool IgnorePath(PathString path, Options options)
        {
            foreach (string ignorePattern in options.IgnorePatterns)
            {
                if (Regex.IsMatch(path, ignorePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public class Options
        {
            public string[] IgnorePatterns { get; set; } = Array.Empty<string>();
        }
    }
}
