# Bazuzi.AzureFunctionInputOutputLogger.AspNetCore

Middleware for Azure Functions (.NET isolated worker model) that logs the query parameters and body of every HTTP request, and the status code and body of its response.

This package is for apps using ASP.NET Core integration (`ConfigureFunctionsWebApplication`, `HttpRequest`, `IActionResult`).
For the built-in HTTP model (`ConfigureFunctionsWorkerDefaults`, `HttpRequestData`, `HttpResponseData`), use [Bazuzi.AzureFunctionInputOutputLogger](https://www.nuget.org/packages/Bazuzi.AzureFunctionInputOutputLogger) instead.


See the [README](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger#readme) for details.
