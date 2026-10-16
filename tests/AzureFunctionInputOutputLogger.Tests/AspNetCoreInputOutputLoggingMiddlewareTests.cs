using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bazuzi.AzureFunctionInputOutputLogger.Tests;

public class AspNetCoreInputOutputLoggingMiddlewareTests
{
    private sealed class Invocation
    {
        public FakeFunctionContext Context { get; } = new();

        public HttpContext? HttpContext { get; set; }

        public object? Result { get; set; }
    }

    private sealed class RecordingMiddleware : IFunctionsWorkerMiddleware
    {
        public bool Called { get; private set; }

        public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            Called = true;
            return next(context);
        }
    }

    private static AspNetCoreInputOutputLoggingMiddleware CreateMiddleware(
        CapturingLogger<InputOutputLoggingMiddleware> logger,
        Invocation invocation,
        bool requireMarkerAttribute = false,
        IFunctionsWorkerMiddleware? fallback = null,
        string[]? redactedQueryParameters = null)
        => new(
            logger,
            requireMarkerAttribute,
            new QueryFormatter(redactedQueryParameters ?? []),
            _ => invocation.HttpContext,
            _ => invocation.Result,
            context => ((FakeFunctionContext)context).MarkedParts,
            fallback ?? new RecordingMiddleware());

    private static DefaultHttpContext CreateHttpContext(string method, string path, string query, string body)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.Request.Path = path;
        httpContext.Request.QueryString = new QueryString(query);
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return httpContext;
    }

    [Fact]
    public async Task LogsObjectResultAsJson()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("POST", "/api/orders", "?a=1&b=two", """{"item":"widget"}"""),
        };

        await CreateMiddleware(logger, invocation).Invoke(invocation.Context, _ =>
        {
            invocation.Result = new ObjectResult(new { Id = 42, Note = "\"quoted\"" }) { StatusCode = 201 };
            return Task.CompletedTask;
        });

        await Verify(logger.Messages);
    }

    [Fact]
    public async Task LogsTheValueThatAspNetCoreLogsOnlyTheTypeOf()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var aspNetCoreLogs = new CapturingLoggerProvider();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("GET", "/api/x", "", ""),
        };
        invocation.HttpContext.RequestServices = new ServiceCollection()
            .AddLogging(logging => logging.AddProvider(aspNetCoreLogs))
            .AddMvcCore().Services
            .BuildServiceProvider();

        // begin-snippet: anonymous-object-result
        var result = new OkObjectResult(new { x = 1, y = "two" });
        // end-snippet

        await CreateMiddleware(logger, invocation).Invoke(invocation.Context, _ =>
        {
            invocation.Result = result;
            return Task.CompletedTask;
        });
        await result.ExecuteResultAsync(new ActionContext(invocation.HttpContext, new RouteData(), new ActionDescriptor()));

        await Verify(new { aspNetCore = aspNetCoreLogs.Messages, thisPackage = logger.Messages });
    }

    [Fact]
    public async Task RedactsFunctionKeyInQuery()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("GET", "/api/orders", "?Code=secret1&code=secret2&name=widget&tag=a&tag=b", ""),
        };

        await CreateMiddleware(logger, invocation).Invoke(invocation.Context, _ =>
        {
            invocation.Result = new OkResult();
            return Task.CompletedTask;
        });

        await Verify(logger.Messages);
    }

    [Fact]
    public async Task RedactsConfiguredQueryParameters()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("GET", "/api/orders", "?ACCESS_TOKEN=token&code=key&sig=signature&id=7", ""),
        };

        await CreateMiddleware(logger, invocation, redactedQueryParameters: ["access_token"]).Invoke(invocation.Context, _ =>
        {
            invocation.Result = new OkResult();
            return Task.CompletedTask;
        });

        await Verify(logger.Messages);
    }

    [Fact]
    public async Task SkipsEntriesWithNothingToLog()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("GET", "/api/orders/7", "", ""),
        };

        await CreateMiddleware(logger, invocation).Invoke(invocation.Context, _ =>
        {
            invocation.Result = new NotFoundResult();
            return Task.CompletedTask;
        });

        Assert.Empty(logger.Messages);
    }

    [Fact]
    public async Task FunctionReadsFullRequestBodyAfterLogging()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("POST", "/api/x", "", "payload"),
        };

        string? seenByFunction = null;
        await CreateMiddleware(logger, invocation).Invoke(invocation.Context, async _ =>
        {
            seenByFunction = await new StreamReader(invocation.HttpContext.Request.Body).ReadToEndAsync();
            invocation.Result = new OkResult();
        });

        await Verify(new { seenByFunction, logger.Messages });
    }

    private sealed record EchoRequest(string Message);

    [Fact]
    public async Task FromBodyBindsFullRequestBodyAfterLogging()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("POST", "/api/x", "", """{"message":"hi"}"""),
        };
        invocation.HttpContext.Request.ContentType = "application/json";

        EchoRequest? boundByFromBody = null;
        await CreateMiddleware(logger, invocation).Invoke(invocation.Context, async _ =>
        {
            // The ASP.NET Core integration binds [FromBody] parameters by deserializing the request body like this.
            boundByFromBody = await invocation.HttpContext.Request.ReadFromJsonAsync<EchoRequest>();
            invocation.Result = new OkObjectResult(new { youSaid = boundByFromBody?.Message });
        });

        await Verify(new { boundByFromBody, logger.Messages });
    }

    [Fact]
    public async Task UnmarkedFunctionIsNotLoggedWhenMarkerIsRequired()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("GET", "/api/x", "?a=1", ""),
        };
        var called = false;

        await CreateMiddleware(logger, invocation, requireMarkerAttribute: true).Invoke(invocation.Context, _ =>
        {
            called = true;
            invocation.Result = new OkObjectResult("hello");
            return Task.CompletedTask;
        });

        Assert.True(called);
        await Verify(logger.Messages);
    }

    [Fact]
    public async Task MarkedFunctionIsLoggedWhenMarkerIsRequired()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("GET", "/api/x", "?a=1", ""),
        };
        invocation.Context.MarkedParts = LoggedParts.InputAndOutput;

        await CreateMiddleware(logger, invocation, requireMarkerAttribute: true).Invoke(invocation.Context, _ =>
        {
            invocation.Result = new OkObjectResult("hello");
            return Task.CompletedTask;
        });

        Assert.Equal(2, logger.Messages.Count);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task LogsOnlyTheMarkedParts(bool input, bool output)
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("POST", "/api/orders", "", """{"item":"widget"}"""),
        };
        invocation.Context.Mark(input, output);

        await CreateMiddleware(logger, invocation, requireMarkerAttribute: true).Invoke(invocation.Context, _ =>
        {
            invocation.Result = new ObjectResult(new { Id = 42 }) { StatusCode = 201 };
            return Task.CompletedTask;
        });

        await Verify(logger.Messages).UseParameters(input, output);
    }

    [Fact]
    public async Task UnrecognizedResultLogsOnlyTheRequest()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation
        {
            HttpContext = CreateHttpContext("GET", "/api/x", "?a=1", ""),
        };

        await CreateMiddleware(logger, invocation).Invoke(invocation.Context, _ =>
        {
            invocation.Result = "not an action result";
            return Task.CompletedTask;
        });

        Assert.Single(logger.Messages);
    }

    [Fact]
    public async Task InvocationsWithoutHttpContextGoToTheFallback()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var invocation = new Invocation();
        var fallback = new RecordingMiddleware();
        var called = false;

        await CreateMiddleware(logger, invocation, fallback: fallback).Invoke(invocation.Context, _ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        Assert.True(fallback.Called);
        Assert.True(called);
        Assert.Empty(logger.Messages);
    }
}
