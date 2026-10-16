# AzureFunctionInputOutputLogger

[![Build and Test](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger/actions/workflows/build-and-test.yml)
[![License](https://img.shields.io/github/license/JayBazuzi/AzureFunctionInputOutputLogger.svg)](LICENSE)

Middleware for Azure Functions (.NET isolated worker model) that logs the query parameters and body of every HTTP request, and the body of its response.
This is meant to address two problems:

1. Instead of a bunch of custom, tedious, repetitive, inconsistent log statements in each function, a single attribute does all the work.
1. If you were counting on the default log message for the response, you are often out of luck because it uses the type name which is useless as a diagnostic.

For more information see [What the host already logs](#what-the-host-already-logs).

## NuGet Packages

There is one package for each of the isolated worker's HTTP models.
Pick the one that matches how your app is configured.

| HTTP model | Your app calls | Functions use | Package |
| --- | --- | --- | --- |
| ASP.NET Core integration | `ConfigureFunctionsWebApplication()` | `HttpRequest`, `IActionResult` | [![NuGet](https://img.shields.io/nuget/v/Bazuzi.AzureFunctionInputOutputLogger.AspNetCore.svg)](https://www.nuget.org/packages/Bazuzi.AzureFunctionInputOutputLogger.AspNetCore) |
| Built-in HTTP model | `ConfigureFunctionsWorkerDefaults()` | `HttpRequestData`, `HttpResponseData` | [![NuGet](https://img.shields.io/nuget/v/Bazuzi.AzureFunctionInputOutputLogger.svg)](https://www.nuget.org/packages/Bazuzi.AzureFunctionInputOutputLogger) |

New function apps created with `func init` use ASP.NET Core integration.

## Usage with ASP.NET Core integration

Add `Bazuzi.AzureFunctionInputOutputLogger.AspNetCore`, then register the middleware after `ConfigureFunctionsWebApplication`:

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

The response entry depends on what the function returns:

- `ObjectResult` (such as `OkObjectResult`): its value serialized as JSON.
- `HttpResponseData`: its body, as with the built-in HTTP model.
- Anything else (such as `NotFoundResult`): no response entry is logged, since there's no body.

For example:

<!-- snippet: tests\AzureFunctionInputOutputLogger.Tests\AspNetCoreInputOutputLoggingMiddlewareTests.LogsObjectResultAsJson.verified.txt -->
<a id='snippet-tests\AzureFunctionInputOutputLogger.Tests\AspNetCoreInputOutputLoggingMiddlewareTests.LogsObjectResultAsJson.verified.txt'></a>
```txt
[
  Information: Request query: a=1&b=two body: {"item":"widget"},
  Information: Response body: {"id":42,"note":"\"quoted\""}
]
```
<sup><a href='#snippet-tests\AzureFunctionInputOutputLogger.Tests\AspNetCoreInputOutputLoggingMiddlewareTests.LogsObjectResultAsJson.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Usage with the built-in HTTP model

Add `Bazuzi.AzureFunctionInputOutputLogger`, then register the middleware when configuring the worker:

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

- The Functions host and Application Insights already record the function name, invocation ID and duration.
- The `requests` table has the path (in `url`, with route values), the HTTP method (in `customDimensions.HttpMethod`) and the status code (in `resultCode`).

They don't record the query string (it isn't part of `requests.url`), the request body or the response body; with ASP.NET Core integration, ASP.NET Core logs only the type of the returned value, not the value itself.

#### Response example

When a function returns this result:

<!-- snippet: anonymous-object-result -->
<a id='snippet-anonymous-object-result'></a>
```cs
var result = new OkObjectResult(new { x = 1, y = "two" });
```
<sup><a href='/tests/AzureFunctionInputOutputLogger.Tests/AspNetCoreInputOutputLoggingMiddlewareTests.cs#L92-L94' title='Snippet source file'>snippet source</a> | <a href='#snippet-anonymous-object-result' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

ASP.NET Core logs only the type of the value, while this package logs the value:

<!-- snippet: tests\AzureFunctionInputOutputLogger.Tests\AspNetCoreInputOutputLoggingMiddlewareTests.LogsTheValueThatAspNetCoreLogsOnlyTheTypeOf.verified.txt -->
<a id='snippet-tests\AzureFunctionInputOutputLogger.Tests\AspNetCoreInputOutputLoggingMiddlewareTests.LogsTheValueThatAspNetCoreLogsOnlyTheTypeOf.verified.txt'></a>
```txt
{
  aspNetCore: [
    Information: Executing OkObjectResult, writing value of type '<>f__AnonymousType1`2[[System.Int32, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[System.String, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]'.
  ],
  thisPackage: [
    Information: Response body: {"x":1,"y":"two"}
  ]
}
```
<sup><a href='#snippet-tests\AzureFunctionInputOutputLogger.Tests\AspNetCoreInputOutputLoggingMiddlewareTests.LogsTheValueThatAspNetCoreLogsOnlyTheTypeOf.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

That gap is what this package fills, so the messages log only the query and bodies, and don't repeat the function name, method, path or status code.

### Queries

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

## Options

To set more than `requireMarkerAttribute`, pass a callback that configures `InputOutputLoggingOptions`:

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

With the built-in HTTP model:

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

- `RequireMarkerAttribute`: log only the functions marked with an attribute, as described below.
- `RedactedQueryParameters`: query parameters whose values are logged as `[redacted]`, matched case-insensitively, in addition to `code`.

## Logging only marked functions

To log only the functions you opt in, require the marker attribute and put `[LogFunctionInputsAndOutputs]` on the function method (or its class).
To log only the request, use `[LogFunctionInputs]`; to log only the response, use `[LogFunctionOutputs]`.
This works the same way in both packages.

<!-- snippet: aspnetcore-require-marker-attribute -->
<a id='snippet-aspnetcore-require-marker-attribute'></a>
```cs
worker.UseAspNetCoreInputOutputLogging(requireMarkerAttribute: true);
```
<sup><a href='/samples/AspNetCoreSampleFunctionApp/RequireMarkerAttributeSample.cs#L11-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-aspnetcore-require-marker-attribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: aspnetcore-marker-attribute-example -->
<a id='snippet-aspnetcore-marker-attribute-example'></a>
```cs
[Function("MyFunction")]
[LogFunctionInputsAndOutputs]
public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest request)
{
```
<sup><a href='/samples/AspNetCoreSampleFunctionApp/MarkedEchoFunction.cs#L10-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-aspnetcore-marker-attribute-example' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

With the built-in HTTP model:

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
[LogFunctionInputsAndOutputs]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request)
{
```
<sup><a href='/samples/SampleFunctionApp/MarkedEchoFunction.cs#L10-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-marker-attribute-example' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Behavior

- Invocations that are not HTTP-triggered are passed through untouched.
- With ASP.NET Core integration, request buffering is enabled so the request body can be logged and then read again by the function.
- With the built-in HTTP model, bodies are only logged when the stream can be rewound, so the function and the host still see them unconsumed.
  Otherwise the log shows `[body not logged: stream is not seekable]` rather than breaking the invocation.

## Build and test

```
./build-and-test.cmd
```
