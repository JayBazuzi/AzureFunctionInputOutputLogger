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

Each HTTP-triggered invocation then produces two `Information` log entries:

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

The response entry depends on what the function returns:

- `ObjectResult` (such as `OkObjectResult`): its status code (200 if unset) and its value serialized as JSON.
- Any other result with a status code (such as `NotFoundResult`): its status code and an empty body.
- `HttpResponseData`: its status code and body, as with the built-in HTTP model.
- Anything else: no response entry is logged.

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

To log only the functions you opt in, require the marker attribute and put `[LogInputOutput]` on the function method (or its class).
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
[LogInputOutput]
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
[LogInputOutput]
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
- Bodies are logged verbatim, so avoid using this where payloads may contain secrets or personal data.
  If some of your functions process sensitive data, disable automatic logging and add `LogInputOutput` only to the functions that are safe to log.

## Build and test

```
./build-and-test.cmd
```
