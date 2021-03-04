namespace Scaffold.WebApi.HttpMessageHandlers
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class RequestLoggingHttpMessageHandler : DelegatingHandler
    {
        private const string RequestStartedMessageTemplate = "Outbound HTTP {HttpMethod} {Uri} started";

        private const string RequestFinishedMessageTemplate = "Outbound HTTP {HttpMethod} {Uri} finished - {StatusCode}";

        private static readonly Action<ILogger, HttpMethod, Uri, Exception?> LogRequestStarted =
            LoggerMessage.Define<HttpMethod, Uri>(LogLevel.Information, default, RequestStartedMessageTemplate);

        private static readonly Action<ILogger, HttpMethod, Uri, int, Exception?> LogRequestFinishedInformation =
            LoggerMessage.Define<HttpMethod, Uri, int>(LogLevel.Information, default, RequestFinishedMessageTemplate);

        private static readonly Action<ILogger, HttpMethod, Uri, int, Exception?> LogRequestFinishedWarning =
            LoggerMessage.Define<HttpMethod, Uri, int>(LogLevel.Warning, default, RequestFinishedMessageTemplate);

        private static readonly Action<ILogger, HttpMethod, Uri, int, Exception?> LogRequestFinishedError =
            LoggerMessage.Define<HttpMethod, Uri, int>(LogLevel.Error, default, RequestFinishedMessageTemplate);

        private static readonly Action<ILogger, HttpMethod, Uri, string, Exception?> LogRequestFinishedCritical =
            LoggerMessage.Define<HttpMethod, Uri, string>(LogLevel.Critical, default, RequestFinishedMessageTemplate);

        private readonly ILogger logger;

        public RequestLoggingHttpMessageHandler(ILogger<RequestLoggingHttpMessageHandler> logger)
        {
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri is null)
            {
                throw new InvalidOperationException("Missing RequestUri while processing request.");
            }

            LogRequestStarted(this.logger, request.Method, request.RequestUri, null);

            try
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                int statusCode = (int)response.StatusCode;

                if (statusCode >= 200 && statusCode <= 299)
                {
                    LogRequestFinishedInformation(this.logger, request.Method, request.RequestUri, statusCode, null);
                    return response;
                }

                if (statusCode >= 500)
                {
                    LogRequestFinishedError(this.logger, request.Method, request.RequestUri, statusCode, null);
                    return response;
                }

                LogRequestFinishedWarning(this.logger, request.Method, request.RequestUri, statusCode, null);

                return response;
            }
            catch (Exception exception)
            {
                LogRequestFinishedCritical(this.logger, request.Method, request.RequestUri, "Unhandled Exception", exception);

                throw;
            }
        }
    }
}
