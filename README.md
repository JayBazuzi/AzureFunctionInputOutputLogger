# AzureFunctionInputOutputLogger

Middleware for Azure Functions (.NET isolated worker model) that logs the query parameters and body of every HTTP request, and the status code and body of its response.

## Usage

Add the package, then register the middleware when configuring the worker:

<!-- snippet: register-middleware -->
<a id='snippet-register-middleware'></a>
```cs
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => worker.UseInputOutputLogging())
    .Build();

host.Run();
```
<sup><a href='/samples/SampleFunctionApp/Program.cs#L4-L10' title='Snippet source file'>snippet source</a> | <a href='#snippet-register-middleware' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Each HTTP-triggered invocation then produces two `Information` log entries:

<!-- snippet: tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt -->
<a id='snippet-tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt'></a>
```txt
[
  Information: Request POST /api/orders query: a=1&b=two body: {"item":"widget"},
  Information: Response 201 body: {"id":42}
]
```
<sup><a href='#snippet-tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Logging only marked functions

To log only the functions you opt in, require the marker attribute and put `[LogInputOutput]` on the function method (or its class):

<!-- snippet: require-marker-attribute -->
<a id='snippet-require-marker-attribute'></a>
```cs
worker.UseInputOutputLogging(requireMarkerAttribute: true);
```
<sup><a href='/samples/SampleFunctionApp/RequireMarkerAttributeSample.cs#L11-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-require-marker-attribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: marker-attribute-example -->
<a id='snippet-marker-attribute-example'></a>
```cs
[Function("MyFunction")]
[LogInputOutput]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request)
{
```
<sup><a href='/samples/SampleFunctionApp/MarkedEchoFunction.cs#L10-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-marker-attribute-example' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Behavior

- Invocations that are not HTTP-triggered are passed through untouched.
- Bodies are only logged when the stream can be rewound, so the function and the host still see them unconsumed. Otherwise the log shows `[body not logged: stream is not seekable]` rather than breaking the invocation.
- Bodies are logged verbatim, so avoid using this where payloads may contain secrets or personal data. If some of your functions process sensitive data, disable automatic logging and add `LogInputOutput` only to the functions that are safe to log.

## Build and test

```
./build-and-test.cmd
```
