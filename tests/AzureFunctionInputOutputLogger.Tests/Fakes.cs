using System.Collections;
using System.Collections.Immutable;
using System.Net;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionInputOutputLogger.Tests;

internal sealed class CapturingLogger<T> : ILogger<T>
{
    public List<string> Messages { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Messages.Add($"{logLevel}: {formatter(state, exception)}");
}

internal sealed class FakeFeatures : IInvocationFeatures
{
    private readonly Dictionary<Type, object> _features = [];

    public T? Get<T>() => _features.TryGetValue(typeof(T), out var value) ? (T)value : default;

    public void Set<T>(T instance) => _features[typeof(T)] = instance!;

    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() => _features.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal sealed class FakeFunctionContext : FunctionContext
{
    private readonly FakeFeatures _features = new();

    public bool IsMarked { get; set; }

    public HttpRequestData? Request { get; set; }

    public HttpResponseData? Response { get; set; }

    public override string InvocationId => "invocation";

    public override string FunctionId => "function";

    public override TraceContext TraceContext => throw new NotSupportedException();

    public override BindingContext BindingContext => throw new NotSupportedException();

    public override RetryContext RetryContext => throw new NotSupportedException();

    public override IServiceProvider InstanceServices { get; set; } = null!;

    public override FunctionDefinition FunctionDefinition => throw new NotSupportedException();

    public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

    public override IInvocationFeatures Features => _features;
}

internal sealed class FakeHttpRequestData(
    FunctionContext context, string method, string url, string body) : HttpRequestData(context)
{
    public override Stream Body { get; } = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(body));

    public override HttpHeadersCollection Headers { get; } = [];

    public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = [];

    public override Uri Url { get; } = new(url);

    public override IEnumerable<ClaimsIdentity> Identities { get; } = [];

    public override string Method { get; } = method;

    public override HttpResponseData CreateResponse() => throw new NotSupportedException();
}

internal sealed class FakeHttpResponseData(FunctionContext context, HttpStatusCode statusCode, string body)
    : HttpResponseData(context)
{
    public override HttpStatusCode StatusCode { get; set; } = statusCode;

    public override HttpHeadersCollection Headers { get; set; } = [];

    public override Stream Body { get; set; } = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(body));

    public override HttpCookies Cookies => throw new NotSupportedException();
}
