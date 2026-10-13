using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace AzureFunctionInputOutputLogger;

/// <summary>
/// Logs the query parameters and body of each HTTP request, and the status code and body of its response.
/// Invocations that are not HTTP-triggered are passed through untouched.
/// </summary>
public sealed class InputOutputLoggingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<InputOutputLoggingMiddleware> _logger;
    private readonly Func<FunctionContext, ValueTask<HttpRequestData?>> _getRequest;
    private readonly Func<FunctionContext, HttpResponseData?> _getResponse;

    public InputOutputLoggingMiddleware(ILogger<InputOutputLoggingMiddleware> logger)
        : this(logger, context => context.GetHttpRequestDataAsync(), context => context.GetHttpResponseData())
    {
    }

    // The SDK's request/response accessors depend on internal types, so tests substitute them here.
    internal InputOutputLoggingMiddleware(
        ILogger<InputOutputLoggingMiddleware> logger,
        Func<FunctionContext, ValueTask<HttpRequestData?>> getRequest,
        Func<FunctionContext, HttpResponseData?> getResponse)
    {
        _logger = logger;
        _getRequest = getRequest;
        _getResponse = getResponse;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await _getRequest(context);
        if (request is null)
        {
            await next(context);
            return;
        }

        _logger.LogInformation(
            "Request {Method} {Path} query: {Query} body: {Body}",
            request.Method,
            request.Url.AbsolutePath,
            FormatQuery(request),
            await ReadBodyAsync(request.Body));

        await next(context);

        var response = _getResponse(context);
        if (response is null)
        {
            return;
        }

        _logger.LogInformation(
            "Response {StatusCode} body: {Body}",
            (int)response.StatusCode,
            await ReadBodyAsync(response.Body));
    }

    private static string FormatQuery(HttpRequestData request) =>
        string.Join(
            "&",
            request.Query.AllKeys
                .Where(key => key is not null)
                .SelectMany(key => (request.Query.GetValues(key) ?? [])
                    .Select(value => $"{key}={value}")));

    // The body is only logged when the stream can be rewound, so that the function (or the host)
    // still sees it unconsumed. Otherwise we note that it was skipped rather than break the invocation.
    private static async Task<string> ReadBodyAsync(Stream? body)
    {
        if (body is null)
        {
            return "";
        }

        if (!body.CanSeek || !body.CanRead)
        {
            return "[body not logged: stream is not seekable]";
        }

        var start = body.Position;
        try
        {
            body.Position = 0;
            using var reader = new StreamReader(body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }
        finally
        {
            body.Position = start;
        }
    }
}
