namespace Scaffold.WebApi.UnitTests.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using FluentValidation;
    using FluentValidation.Results;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using OpenTracing.Mock;
    using Scaffold.Application.Base;
    using Scaffold.Domain.Base;
    using Scaffold.WebApi.Filters;
    using Xunit;

    public class ExceptionFilterUnitTests
    {
        private readonly ActionContext actionContext;

        public ExceptionFilterUnitTests()
        {
            this.actionContext = new ActionContext
            {
                ActionDescriptor = new ActionDescriptor(),
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
            };
        }

        public class Constructor
        {
            [Fact]
            public void When_InstantiatingExceptionFilterWithNullTracer_Expect_ArgumentNullException()
            {
                // Act
                Exception exception = Record.Exception(() => new ExceptionFilter(null!));

                // Assert
                Assert.IsType<ArgumentNullException>(exception);
            }
        }

        public class OnActionExecuted : ExceptionFilterUnitTests
        {
            [Fact]
            public void When_HandlingObjectResultWithProblemDetails_Expect_ProblemDetailsWithTraceId()
            {
                // Arrange
                MockTracer mockTracer = new MockTracer();

                ActionExecutedContext context = new ActionExecutedContext(this.actionContext, new List<IFilterMetadata>(), null)
                {
                    Result = new ObjectResult(new ProblemDetails()),
                };

                ExceptionFilter exceptionFilter = new ExceptionFilter(mockTracer);

                // Act
                using (mockTracer.BuildSpan("Unit Test").StartActive())
                {
                    exceptionFilter.OnActionExecuted(context);
                }

                // Assert
                ObjectResult objectResult = Assert.IsType<ObjectResult>(context.Result);
                ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
                MockSpan mockSpan = Assert.Single(mockTracer.FinishedSpans());
                Assert.Equal(mockSpan.Context.TraceId, problemDetails.Extensions["traceId"]);
            }
        }

        public class OnActionExecuting : ExceptionFilterUnitTests
        {
            [Fact]
            public void When_ActionExecuting_Expect_NullContextResult()
            {
                // Arrange
                ActionExecutingContext context = new ActionExecutingContext(
                    this.actionContext,
                    new List<IFilterMetadata>(),
                    new Dictionary<string, object>(),
                    null);

                ExceptionFilter exceptionFilter = new ExceptionFilter(new MockTracer());

                // Act
                exceptionFilter.OnActionExecuting(context);

                // Assert
                Assert.Empty(context.ActionArguments);
                Assert.Null(context.Controller);
                Assert.Empty(context.Filters);
                Assert.Null(context.Result);
            }
        }

        public class OnException : ExceptionFilterUnitTests
        {
            [Fact]
            public void When_HandlingDomainException_Expect_ConflictObjectResult()
            {
                // Arrange
                ExceptionContext context = new ExceptionContext(this.actionContext, new List<IFilterMetadata>())
                {
                    Exception = new TestDomainException(Guid.NewGuid().ToString()),
                };

                ExceptionFilter exceptionFilter = new ExceptionFilter(new MockTracer());

                // Act
                exceptionFilter.OnException(context);

                // Assert
                ConflictObjectResult objectResult = Assert.IsType<ConflictObjectResult>(context.Result);
                Assert.IsType<ProblemDetails>(objectResult.Value);
            }

            [Fact]
            public void When_HandlingNotFoundException_Expect_NotFoundObjectResult()
            {
                // Arrange
                ExceptionContext context = new ExceptionContext(this.actionContext, new List<IFilterMetadata>())
                {
                    Exception = new TestNotFoundException(Guid.NewGuid().ToString()),
                };

                ExceptionFilter exceptionFilter = new ExceptionFilter(new MockTracer());

                // Act
                exceptionFilter.OnException(context);

                // Assert
                NotFoundObjectResult objectResult = Assert.IsType<NotFoundObjectResult>(context.Result);
                Assert.IsType<ProblemDetails>(objectResult.Value);
            }

            [Fact]
            public void When_HandlingValidationException_Expect_BadRequestObjectResult()
            {
                // Arrange
                List<ValidationFailure> validationFailures = new List<ValidationFailure>
                {
                    new ValidationFailure("property1", "Error Message."),
                    new ValidationFailure("property1", "Another Error Message."),
                    new ValidationFailure("property1", "One More Error Message."),
                    new ValidationFailure("property2", "Error Message."),
                    new ValidationFailure("property3", "Error Message."),
                };

                ExceptionContext context = new ExceptionContext(this.actionContext, new List<IFilterMetadata>())
                {
                    Exception = new ValidationException(Guid.NewGuid().ToString(), validationFailures),
                };

                ExceptionFilter exceptionFilter = new ExceptionFilter(new MockTracer());

                // Act
                exceptionFilter.OnException(context);

                // Assert
                BadRequestObjectResult objectResult = Assert.IsType<BadRequestObjectResult>(context.Result);
                ValidationProblemDetails problemDetails = Assert.IsType<ValidationProblemDetails>(objectResult.Value);

                Assert.Equal(3, problemDetails.Errors.Count);
                Assert.Equal(3, problemDetails.Errors["property1"].Length);
                Assert.Single(problemDetails.Errors["property2"]);
                Assert.Single(problemDetails.Errors["property3"]);
            }

            [Fact]
            public void When_HandlingUnhandledException_Expect_NullContextResult()
            {
                // Arrange
                ExceptionContext context = new ExceptionContext(this.actionContext, new List<IFilterMetadata>())
                {
                    Exception = new Exception(),
                };

                ExceptionFilter exceptionFilter = new ExceptionFilter(new MockTracer());

                // Act
                exceptionFilter.OnException(context);

                // Assert
                Assert.Null(context.Result);
            }

            [Fact]
            public void When_HandlingExpection_Expect_ProblemDetailsWithTraceId()
            {
                // Arrange
                MockTracer mockTracer = new MockTracer();

                ExceptionContext context = new ExceptionContext(this.actionContext, new List<IFilterMetadata>())
                {
                    Result = new ObjectResult(new ProblemDetails()),
                };

                ExceptionFilter exceptionFilter = new ExceptionFilter(mockTracer);

                // Act
                using (mockTracer.BuildSpan("Unit Test").StartActive())
                {
                    exceptionFilter.OnException(context);
                }

                // Assert
                ObjectResult objectResult = Assert.IsType<ObjectResult>(context.Result);
                ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
                MockSpan mockSpan = Assert.Single(mockTracer.FinishedSpans());
                Assert.Equal(mockSpan.Context.TraceId, problemDetails.Extensions["traceId"]);
            }
        }

        [Serializable]
        private class TestDomainException : DomainException
        {
            public TestDomainException(string message)
                : base(message)
            {
            }

            public TestDomainException(string message, Exception innerException)
                : base(message, innerException)
            {
            }

            public TestDomainException(string title, string message)
                : base(title, message)
            {
            }

            public TestDomainException(string title, string message, Exception innerException)
                : base(title, message, innerException)
            {
            }

            protected TestDomainException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }

        [Serializable]
        private class TestNotFoundException : NotFoundException
        {
            public TestNotFoundException(string message)
                : base(message)
            {
            }

            public TestNotFoundException(string message, Exception innerException)
                : base(message, innerException)
            {
            }

            public TestNotFoundException(string title, string message)
                : base(title, message)
            {
            }

            public TestNotFoundException(string title, string message, Exception innerException)
                : base(title, message, innerException)
            {
            }

            protected TestNotFoundException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }
    }
}
