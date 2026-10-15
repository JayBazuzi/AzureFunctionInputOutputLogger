using System.Collections.Concurrent;
using System.Reflection;
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
internal sealed class InputOutputLoggingMiddleware : IFunctionsWorkerMiddleware
{
    private static readonly ConcurrentDictionary<(string? Assembly, string? EntryPoint), bool> MarkerCache = new();

    private readonly ILogger<InputOutputLoggingMiddleware> _logger;
    private readonly bool _requireMarkerAttribute;
    private readonly Func<FunctionContext, ValueTask<HttpRequestData?>> _getRequest;
    private readonly Func<FunctionContext, HttpResponseData?> _getResponse;
    private readonly Func<FunctionContext, bool> _isMarked;

    public InputOutputLoggingMiddleware(
        ILogger<InputOutputLoggingMiddleware> logger,
        bool requireMarkerAttribute)
        : this(
            logger,
            requireMarkerAttribute,
            context => context.GetHttpRequestDataAsync(),
            context => context.GetHttpResponseData(),
            HasMarker)
    {
    }

    // The SDK's request/response accessors depend on internal types, so tests substitute them here.
    internal InputOutputLoggingMiddleware(
        ILogger<InputOutputLoggingMiddleware> logger,
        bool requireMarkerAttribute,
        Func<FunctionContext, ValueTask<HttpRequestData?>> getRequest,
        Func<FunctionContext, HttpResponseData?> getResponse,
        Func<FunctionContext, bool> isMarked)
    {
        _logger = logger;
        _requireMarkerAttribute = requireMarkerAttribute;
        _getRequest = getRequest;
        _getResponse = getResponse;
        _isMarked = isMarked;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        if (_requireMarkerAttribute && !_isMarked(context))
        {
            await next(context);
            return;
        }

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

    // The entry point is "Namespace.Class.Method"; the attribute may be on the method or its class.
    internal static bool HasMarker(FunctionContext context)
    {
        var definition = context.FunctionDefinition;
        return MarkerCache.GetOrAdd((definition.PathToAssembly, definition.EntryPoint), key =>
        {
            if (key.Assembly is null || key.EntryPoint is null)
            {
                return false;
            }

            var split = key.EntryPoint.LastIndexOf('.');
            if (split < 0)
            {
                return false;
            }

            var assemblyName = AssemblyName.GetAssemblyName(key.Assembly);
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), assemblyName))
                ?? Assembly.LoadFrom(key.Assembly);

            var type = assembly.GetType(key.EntryPoint[..split]);
            if (type is null)
            {
                return false;
            }

            var method = type.GetMethod(
                key.EntryPoint[(split + 1)..],
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            return type.IsDefined(typeof(LogInputOutputAttribute), inherit: true)
                || (method?.IsDefined(typeof(LogInputOutputAttribute), inherit: true) ?? false);
        });
    }

    private static string FormatQuery(HttpRequestData request)
        => string.Join(
            "&",
            request.Query.AllKeys
                .Where(key => key is not null)
                .SelectMany(key => (request.Query.GetValues(key) ?? [])
                    .Select(value => $"{key}={value}")));

    // The body is only logged when the stream can be rewound, so that the function (or the host)
    // still sees it unconsumed. Otherwise we note that it was skipped rather than break the invocation.
    internal static async Task<string> ReadBodyAsync(Stream? body)
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
