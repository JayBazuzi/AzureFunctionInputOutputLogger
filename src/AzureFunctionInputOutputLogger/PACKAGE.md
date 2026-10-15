# Bazuzi.AzureFunctionInputOutputLogger

Middleware for Azure Functions (.NET isolated worker model) that logs the query parameters and body of every HTTP request, and the status code and body of its response.

This package is for apps using the built-in HTTP model (`ConfigureFunctionsWorkerDefaults`, `HttpRequestData`, `HttpResponseData`).
For ASP.NET Core integration (`ConfigureFunctionsWebApplication`, `HttpRequest`, `IActionResult`), use `Bazuzi.AzureFunctionInputOutputLogger.AspNetCore` instead.

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

See the [README](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger#readme) for details.
