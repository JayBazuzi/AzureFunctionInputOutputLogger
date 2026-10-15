using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Bazuzi.AzureFunctionInputOutputLogger;

/// <summary>
/// Logs HTTP requests and responses for apps using ASP.NET Core integration (<c>ConfigureFunctionsWebApplication</c>).
/// Invocations without an ASP.NET Core <see cref="HttpContext"/> are handled by <see cref="InputOutputLoggingMiddleware"/>.
/// </summary>
internal sealed class AspNetCoreInputOutputLoggingMiddleware : IFunctionsWorkerMiddleware
{
    // Matches how ASP.NET Core writes JSON (camelCase), without the HTML-safe escaping that makes logs hard to read.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly ILogger<InputOutputLoggingMiddleware> _logger;
    private readonly bool _requireMarkerAttribute;
    private readonly QueryFormatter _queryFormatter;
    private readonly Func<FunctionContext, HttpContext?> _getHttpContext;
    private readonly Func<FunctionContext, object?> _getInvocationResult;
    private readonly Func<FunctionContext, LoggedParts> _getMarkedParts;
    private readonly IFunctionsWorkerMiddleware _fallback;

    public AspNetCoreInputOutputLoggingMiddleware(
        ILogger<InputOutputLoggingMiddleware> logger,
        bool requireMarkerAttribute,
        QueryFormatter queryFormatter)
        : this(
            logger,
            requireMarkerAttribute,
            queryFormatter,
            context => context.GetHttpContext(),
            context => context.GetInvocationResult().Value,
            InputOutputLoggingMiddleware.GetMarkedParts,
            new InputOutputLoggingMiddleware(logger, requireMarkerAttribute, queryFormatter))
    {
    }

    // The SDK's accessors depend on internal types, so tests substitute them here.
    internal AspNetCoreInputOutputLoggingMiddleware(
        ILogger<InputOutputLoggingMiddleware> logger,
        bool requireMarkerAttribute,
        QueryFormatter queryFormatter,
        Func<FunctionContext, HttpContext?> getHttpContext,
        Func<FunctionContext, object?> getInvocationResult,
        Func<FunctionContext, LoggedParts> getMarkedParts,
        IFunctionsWorkerMiddleware fallback)
    {
        _logger = logger;
        _requireMarkerAttribute = requireMarkerAttribute;
        _queryFormatter = queryFormatter;
        _getHttpContext = getHttpContext;
        _getInvocationResult = getInvocationResult;
        _getMarkedParts = getMarkedParts;
        _fallback = fallback;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = _getHttpContext(context);
        if (httpContext is null)
        {
            await _fallback.Invoke(context, next);
            return;
        }

        var parts = _requireMarkerAttribute ? _getMarkedParts(context) : LoggedParts.InputAndOutput;
        if (parts == LoggedParts.None)
        {
            await next(context);
            return;
        }

        if (parts.HasFlag(LoggedParts.Input))
        {
            var request = httpContext.Request;
            request.EnableBuffering();

            _logger.LogInformation(
                "Request {Method} {Path} query: {Query} body: {Body}",
                request.Method,
                request.Path.Value,
                FormatQuery(request),
                await InputOutputLoggingMiddleware.ReadBodyAsync(request.Body));
        }

        await next(context);

        if (!parts.HasFlag(LoggedParts.Output))
        {
            return;
        }

        var response = await DescribeResultAsync(_getInvocationResult(context));
        if (response is null)
        {
            return;
        }

        _logger.LogInformation(
            "Response {StatusCode} body: {Body}",
            response.Value.StatusCode,
            response.Value.Body);
    }

    // Results that carry no status code (anything that isn't an IActionResult with one, or HttpResponseData) are not logged.
    private static async Task<(int StatusCode, string Body)?> DescribeResultAsync(object? result)
        => result switch
        {
            ObjectResult objectResult => (objectResult.StatusCode ?? StatusCodes.Status200OK, JsonSerializer.Serialize(objectResult.Value, JsonOptions)),
            IStatusCodeActionResult statusCodeResult => (statusCodeResult.StatusCode ?? StatusCodes.Status200OK, ""),
            HttpResponseData responseData => ((int)responseData.StatusCode, await InputOutputLoggingMiddleware.ReadBodyAsync(responseData.Body)),
            _ => null,
        };

    private string FormatQuery(HttpRequest request)
        => _queryFormatter.Format(
            request.Query.SelectMany(pair => pair.Value.Select(value => (pair.Key, value))));
}
