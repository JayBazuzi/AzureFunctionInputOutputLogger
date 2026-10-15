# Bazuzi.AzureFunctionInputOutputLogger.AspNetCore

Middleware for Azure Functions (.NET isolated worker model) that logs the query parameters and body of every HTTP request, and the body of its response.

This package is for apps using ASP.NET Core integration (`ConfigureFunctionsWebApplication`, `HttpRequest`, `IActionResult`).
For the built-in HTTP model (`ConfigureFunctionsWorkerDefaults`, `HttpRequestData`, `HttpResponseData`), use [Bazuzi.AzureFunctionInputOutputLogger](https://www.nuget.org/packages/Bazuzi.AzureFunctionInputOutputLogger) instead.

## Usage

<!-- snippet: register-aspnetcore-middleware -->
<a id='snippet-register-aspnetcore-middleware'></a>
```cs
var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.UseAspNetCoreInputOutputLogging(requireMarkerAttribute: false);

builder.Build().Run();
```
<sup><a href='/samples/AspNetCoreSampleFunctionApp/Program.cs#L5-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-register-aspnetcore-middleware' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To redact more query parameters, or to log only marked functions, configure `InputOutputLoggingOptions`:

<!-- snippet: aspnetcore-redact-query-parameters -->
<a id='snippet-aspnetcore-redact-query-parameters'></a>
```cs
worker.UseAspNetCoreInputOutputLogging(options =>
{
    options.RequireMarkerAttribute = true;
    options.RedactedQueryParameters.Add("access_token");
    options.RedactedQueryParameters.Add("sig");
});
```
<sup><a href='/samples/AspNetCoreSampleFunctionApp/RedactQueryParametersSample.cs#L11-L18' title='Snippet source file'>snippet source</a> | <a href='#snippet-aspnetcore-redact-query-parameters' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## What gets logged<!-- include: log-output. path: /docs/log-output.include.md -->

Each HTTP-triggered invocation produces up to two `Information` log entries, one for the request and one for the response, with these message templates:

- `Request query: {Query} body: {Body}`, or `Request query: {Query}` or `Request body: {Body}` when the other part is empty
- `Response body: {Body}`

An entry with nothing to log (a request with no query and no body, or a response with an empty body) is skipped.

For example:

<!-- snippet: tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt -->
<a id='snippet-tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt'></a>
```txt
[
  Information: Request query: code=[redacted]&a=1&b=two body: {"item":"widget"},
  Information: Response body: {"id":42}
]
```
<sup><a href='#snippet-tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The value of the `code` query parameter, which carries the Azure Functions key, is always logged as `[redacted]`.
To redact other query parameters, such as `access_token` or `sig`, add them to `RedactedQueryParameters`.

The logger category is `Bazuzi.AzureFunctionInputOutputLogger.InputOutputLoggingMiddleware` for every function, in both packages.
In Application Insights, find the entries for a function or an invocation with `operation_Name` or `customDimensions.InvocationId` rather than with the category.

### What the host already logs

The Functions host and Application Insights already record the function name, invocation ID and duration.
The `requests` table has the path (in `url`, with route values), the HTTP method (in `customDimensions.HttpMethod`) and the status code (in `resultCode`).
They don't record the query string (it isn't part of `requests.url`), the request body or the response body; with ASP.NET Core integration the host logs only the result's type, not its value.
That gap is what this package fills, so the messages log only the query and bodies, and don't repeat the function name, method, path or status code.

To see them together, join the log entries to `requests` on `operation_Id`:

<!-- snippet: docs\join-requests.kql -->
<a id='snippet-docs\join-requests.kql'></a>
```kql
traces
| where operation_Name == "EchoFunction"
| where message startswith "Request " or message startswith "Response "
| join kind=inner (
    requests
    | project operation_Id, method = tostring(customDimensions.HttpMethod), url, resultCode
  ) on operation_Id
| project timestamp, method, url, resultCode, message
```
<sup><a href='#snippet-docs\join-requests.kql' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Keeping sensitive endpoints out of the logs

Bodies are logged verbatim.
To keep sensitive endpoints, such as payment or account endpoints, out of the logs, set `RequireMarkerAttribute` (or pass `requireMarkerAttribute: true`) and mark only the functions that are safe to log with `[LogFunctionInputs]`, `[LogFunctionOutputs]` or `[LogFunctionInputsAndOutputs]`.
Unmarked functions are not logged at all.<!-- endInclude -->

See the [README](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger#readme) for details.
