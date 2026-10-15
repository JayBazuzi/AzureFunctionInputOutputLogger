# Bazuzi.AzureFunctionInputOutputLogger

Middleware for Azure Functions (.NET isolated worker model) that logs the query parameters and body of every HTTP request, and the status code and body of its response.

This package is for apps using the built-in HTTP model (`ConfigureFunctionsWorkerDefaults`, `HttpRequestData`, `HttpResponseData`).
For ASP.NET Core integration (`ConfigureFunctionsWebApplication`, `HttpRequest`, `IActionResult`), use [Bazuzi.AzureFunctionInputOutputLogger.AspNetCore](https://www.nuget.org/packages/Bazuzi.AzureFunctionInputOutputLogger.AspNetCore) instead.

## Usage

<!-- snippet: register-middleware -->
<a id='snippet-register-middleware'></a>
```cs
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => worker.UseInputOutputLogging(requireMarkerAttribute: false))
    .Build();

host.Run();
```
<sup><a href='/samples/SampleFunctionApp/Program.cs#L4-L10' title='Snippet source file'>snippet source</a> | <a href='#snippet-register-middleware' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To redact more query parameters, or to log only marked functions, configure `InputOutputLoggingOptions`:

<!-- snippet: redact-query-parameters -->
<a id='snippet-redact-query-parameters'></a>
```cs
worker.UseInputOutputLogging(options =>
{
    options.RequireMarkerAttribute = true;
    options.RedactedQueryParameters.Add("access_token");
    options.RedactedQueryParameters.Add("sig");
});
```
<sup><a href='/samples/SampleFunctionApp/RedactQueryParametersSample.cs#L11-L18' title='Snippet source file'>snippet source</a> | <a href='#snippet-redact-query-parameters' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## What gets logged<!-- include: log-output. path: /docs/log-output.include.md -->

Each HTTP-triggered invocation produces two `Information` log entries, with these message templates:

- `Request {Method} {Path} query: {Query} body: {Body}`
- `Response {StatusCode} body: {Body}`

For example:

<!-- snippet: tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt -->
<a id='snippet-tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt'></a>
```txt
[
  Information: Request POST /api/orders query: code=[redacted]&a=1&b=two body: {"item":"widget"},
  Information: Response 201 body: {"id":42}
]
```
<sup><a href='#snippet-tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The value of the `code` query parameter, which carries the Azure Functions key, is always logged as `[redacted]`.
To redact other query parameters, such as `access_token` or `sig`, add them to `RedactedQueryParameters`.

The logger category is `Bazuzi.AzureFunctionInputOutputLogger.InputOutputLoggingMiddleware` for every function, in both packages.
In Application Insights, find the entries for a function or an invocation with `operation_Name` or `customDimensions.InvocationId` rather than with the category.

The Functions host and Application Insights already record the function name, invocation ID and duration, and the `requests` table has the path (with route values) and status code.
They don't record the query string (it isn't part of `requests.url`), the request body or the response body; with ASP.NET Core integration the host logs only the result's type, not its value.
That gap is what this package fills, so the messages don't repeat the function name.

### Keeping sensitive endpoints out of the logs

Bodies are logged verbatim.
To keep sensitive endpoints, such as payment or account endpoints, out of the logs, set `RequireMarkerAttribute` (or pass `requireMarkerAttribute: true`) and mark only the functions that are safe to log with `[LogFunctionInputs]`, `[LogFunctionOutputs]` or `[LogFunctionInputsAndOutputs]`.
Unmarked functions are not logged at all.<!-- endInclude -->

See the [README](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger#readme) for details.
