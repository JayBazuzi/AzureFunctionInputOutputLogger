# Bazuzi.AzureFunctionInputOutputLogger.AspNetCore

Middleware for Azure Functions (.NET isolated worker model) that logs the route parameters, query parameters, and body of every HTTP request, and the body of its response.

This package is for apps using ASP.NET Core integration (`ConfigureFunctionsWebApplication`, `HttpRequest`, `IActionResult`).
For the built-in HTTP model (`ConfigureFunctionsWorkerDefaults`, `HttpRequestData`, `HttpResponseData`), use [Bazuzi.AzureFunctionInputOutputLogger](https://www.nuget.org/packages/Bazuzi.AzureFunctionInputOutputLogger) instead.

See the [README](https://github.com/JayBazuzi/AzureFunctionInputOutputLogger#readme) for usage, what gets logged and how to keep sensitive endpoints out of the logs.
