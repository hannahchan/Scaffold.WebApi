namespace Scaffold.WebApi.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class TracingDemoController : ControllerBase
    {
        private readonly IClient tracingDemoClient;

        public TracingDemoController(IClient tracingDemoClient)
        {
            this.tracingDemoClient = tracingDemoClient;
        }

        public interface IClient
        {
            Task<HttpResponseMessage> Get(Uri uri);
        }

        /// <summary>Makes a proxy request to the 'Hello' endpoint.</summary>
        /// <param name="name">The name of the person to say 'Hello' to.</param>
        /// <returns>A string containing the 'Hello' message.</returns>
        /// <response code="200">Message was successful.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<string> Proxy([FromQuery] string? name)
        {
            HttpRequest request = this.HttpContext.Request;
            Uri uri = new Uri($"{request.Scheme}://{request.Host}/TracingDemo/Hello?name={name ?? "random"}", UriKind.Absolute);

            HttpResponseMessage response = await this.tracingDemoClient.Get(uri);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>Returns a 'Hello' message or 'Service Unavailable'.</summary>
        /// <param name="name">The name of the person to say 'Hello' to.</param>
        /// <returns>A string containing the 'Hello' message.</returns>
        /// <response code="200">Message was successful.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpGet("Hello")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<string>> Hello([FromQuery] string? name)
        {
            await Task.Delay(new Random().Next(500));

            if (new Random().NextDouble() <= 0.8)
            {
                return this.Problem(
                    type: "https://tools.ietf.org/html/rfc7231#section-6.6.4",
                    title: "Service Unavailable",
                    detail: "This is intended 80% of the time. Please try again.",
                    statusCode: (int)HttpStatusCode.ServiceUnavailable);
            }

            return $"Hello, {name ?? "random"}!";
        }

        public class Client : IClient
        {
            private readonly HttpClient httpClient;

            public Client(HttpClient httpClient)
            {
                this.httpClient = httpClient;
            }

            public Task<HttpResponseMessage> Get(Uri uri)
            {
                return this.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
            }
        }
    }
}
