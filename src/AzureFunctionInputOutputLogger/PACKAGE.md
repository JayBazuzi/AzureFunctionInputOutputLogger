# Bazuzi.AzureFunctionInputOutputLogger

Middleware for Azure Functions (.NET isolated worker model) that logs the query parameters and body of every HTTP request, and the status code and body of its response.

This package is for apps using the built-in HTTP model (`ConfigureFunctionsWorkerDefaults`, `HttpRequestData`, `HttpResponseData`).
For ASP.NET Core integration (`ConfigureFunctionsWebApplication`, `HttpRequest`, `IActionResult`), use [Bazuzi.AzureFunctionInputOutputLogger.AspNetCore](https://www.nuget.org/packages/Bazuzi.AzureFunctionInputOutputLogger.AspNetCore) instead.

## Usage

<!-- snippet: register-middleware -->
<!-- endSnippet -->

See the [README](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger#readme) for details.
