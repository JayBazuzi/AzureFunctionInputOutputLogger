# Bazuzi.AzureFunctionInputOutputLogger.AspNetCore

Middleware for Azure Functions (.NET isolated worker model) that logs the query parameters and body of every HTTP request, and the status code and body of its response.

This package is for apps using ASP.NET Core integration (`ConfigureFunctionsWebApplication`, `HttpRequest`, `IActionResult`).
For the built-in HTTP model (`ConfigureFunctionsWorkerDefaults`, `HttpRequestData`, `HttpResponseData`), use `Bazuzi.AzureFunctionInputOutputLogger` instead.

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

See the [README](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger#readme) for details.
