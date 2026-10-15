# AzureFunctionInputOutputLogger

[![Build and Test](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger/actions/workflows/build-and-test.yml)
[![License](https://img.shields.io/github/license/JayBazuzi/AzureFunctionInputOutputLogger.svg)](LICENSE)

Middleware for Azure Functions (.NET isolated worker model) that logs the query parameters and body of every HTTP request, and the status code and body of its response.

## Packages

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

- `ObjectResult` (such as `OkObjectResult`): its status code (200 if unset) and its value serialized as JSON.
- Any other result with a status code (such as `NotFoundResult`): its status code and an empty body.
- `HttpResponseData`: its status code and body, as with the built-in HTTP model.
- Anything else: no response entry is logged.

For example:

<!-- snippet: tests\AzureFunctionInputOutputLogger.Tests\AspNetCoreInputOutputLoggingMiddlewareTests.LogsObjectResultAsJson.verified.txt -->
<a id='snippet-tests\AzureFunctionInputOutputLogger.Tests\AspNetCoreInputOutputLoggingMiddlewareTests.LogsObjectResultAsJson.verified.txt'></a>
```txt
[
  Information: Request POST /api/orders query: a=1&b=two body: {"item":"widget"},
  Information: Response 201 body: {"id":42,"note":"\"quoted\""}
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
