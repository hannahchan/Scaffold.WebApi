namespace Scaffold.WebApi.UnitTests.Middleware
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Scaffold.WebApi.Middleware;
    using Xunit;

    public class RequestLoggingMiddlewareUnitTests
    {
        [Theory]
        [InlineData(199, LogLevel.Warning)]
        [InlineData(200, LogLevel.Information)]
        [InlineData(299, LogLevel.Information)]
        [InlineData(300, LogLevel.Warning)]
        [InlineData(499, LogLevel.Warning)]
        [InlineData(500, LogLevel.Error)]
        [InlineData(501, LogLevel.Error)]
        public async Task When_InvokingMiddlewareWithStatusCode_Expect_LogLevel(int statusCode, LogLevel expectedLogLevel)
        {
            // Arrange
            Mock<ILogger<RequestLoggingMiddleware>> mock = new Mock<ILogger<RequestLoggingMiddleware>>();

            RequestLoggingMiddleware middleware = new RequestLoggingMiddleware(
                (httpContext) => Task.CompletedTask,
                new TestHostingEnvironment { ApplicationName = "Unit Test", EnvironmentName = "Production" },
                mock.Object);

            HttpContext context = new DefaultHttpContext();
            context.Response.StatusCode = statusCode;

            // Act
            await middleware.Invoke(context);

            // Assert
            mock.Verify(
                m => m.Log(
                    expectedLogLevel,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    null,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("/health")]
        [InlineData("/HEALTH")]
        [InlineData("/hEaLtH")]
        public async Task When_InvokingMiddlewareWithHealthCheckRequestPath_Expect_LogLevelDebug(string path)
        {
            // Arrange
            Mock<ILogger<RequestLoggingMiddleware>> mock = new Mock<ILogger<RequestLoggingMiddleware>>();

            RequestLoggingMiddleware middleware = new RequestLoggingMiddleware(
                (httpContext) => Task.CompletedTask,
                new TestHostingEnvironment { ApplicationName = "Unit Test", EnvironmentName = "Production" },
                mock.Object);

            HttpContext context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Response.StatusCode = new Random().Next(100, 512);

            // Act
            await middleware.Invoke(context);

            // Assert
            mock.Verify(
                m => m.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    null,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task When_InvokingMiddlewareInDevelopmentWithException_Expect_LogLevelCritical()
        {
            // Arrange
            Exception exception = new Exception("Unit Test");

            Mock<ILogger<RequestLoggingMiddleware>> mock = new Mock<ILogger<RequestLoggingMiddleware>>();

            RequestLoggingMiddleware middleware = new RequestLoggingMiddleware(
                (httpContext) => throw exception,
                new TestHostingEnvironment { ApplicationName = "Unit Test", EnvironmentName = "Development" },
                mock.Object);

            HttpContext context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            // Assert
            mock.Verify(
                m => m.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    exception,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);

            Assert.Equal("text/plain", context.Response.ContentType);
            Assert.Equal(500, context.Response.StatusCode);

            using (StreamReader reader = new StreamReader(context.Response.Body))
            {
                context.Response.Body.Position = 0;
                Assert.Equal(exception.ToString(), reader.ReadToEnd());
            }
        }

        [Fact]
        public async Task When_InvokingMiddlewareInProductionWithException_Expect_LogLevelCritical()
        {
            // Arrange
            Exception exception = new Exception("Unit Test");

            Mock<ILogger<RequestLoggingMiddleware>> mock = new Mock<ILogger<RequestLoggingMiddleware>>();

            RequestLoggingMiddleware middleware = new RequestLoggingMiddleware(
                (httpContext) => throw exception,
                new TestHostingEnvironment { ApplicationName = "Unit Test", EnvironmentName = "Production" },
                mock.Object);

            HttpContext context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            // Assert
            mock.Verify(
                m => m.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    exception,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);

            Assert.Equal("text/plain", context.Response.ContentType);
            Assert.Equal(500, context.Response.StatusCode);

            using (StreamReader reader = new StreamReader(context.Response.Body))
            {
                context.Response.Body.Position = 0;
                Assert.Equal("Oh no! Something has gone wrong.", reader.ReadToEnd());
            }
        }

        private class TestHostingEnvironment : IHostingEnvironment
        {
            public string EnvironmentName { get; set; }

            public string ApplicationName { get; set; }

            public string WebRootPath { get; set; }

            public IFileProvider WebRootFileProvider { get; set; }

            public string ContentRootPath { get; set; }

            public IFileProvider ContentRootFileProvider { get; set; }
        }
    }
}