# AzureFunctionInputOutputLogger

Middleware for Azure Functions (.NET isolated worker model) that logs the query parameters and body of every HTTP request, and the status code and body of its response.

## Usage

Add the package, then register the middleware when configuring the worker:

<!-- snippet: register-middleware -->
```cs
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => worker.UseInputOutputLogging())
    .Build();

host.Run();
```
<!-- endSnippet -->

Each HTTP-triggered invocation then produces two `Information` log entries:

include: tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt

## Behavior

- Invocations that are not HTTP-triggered are passed through untouched.
- Bodies are only logged when the stream can be rewound, so the function and the host still see them unconsumed. Otherwise the log shows `[body not logged: stream is not seekable]` rather than breaking the invocation.
- Bodies are logged verbatim, so avoid using this where payloads may contain secrets or personal data.

## Sample

[samples/SampleFunctionApp](samples/SampleFunctionApp) is a minimal function app that echoes its request body with logging enabled.

## Build and test

```
./build-and-test.cmd
```
