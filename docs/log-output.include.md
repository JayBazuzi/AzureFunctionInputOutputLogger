## What gets logged

Each HTTP-triggered invocation produces up to two `Information` log entries, one for the request and one for the response, with these message templates:

- `Request query: {Query} body: {Body}`, or `Request query: {Query}` or `Request body: {Body}` when the other part is empty
- `Response body: {Body}`

An entry with nothing to log (a request with no query and no body, or a response with an empty body) is skipped.

For example:

snippet: tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt

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

snippet: anonymous-object-result

ASP.NET Core logs only the type of the value, while this package logs the value:

snippet: tests\AzureFunctionInputOutputLogger.Tests\AspNetCoreInputOutputLoggingMiddlewareTests.LogsTheValueThatAspNetCoreLogsOnlyTheTypeOf.verified.txt

That gap is what this package fills, so the messages log only the query and bodies, and don't repeat the function name, method, path or status code.

### Queries

To see them together, join the log entries to `requests` on `operation_Id`:

snippet: docs\join-requests.kql

### Keeping sensitive endpoints out of the logs

Bodies are logged verbatim.
To keep sensitive endpoints, such as payment or account endpoints, out of the logs, set `RequireMarkerAttribute` (or pass `requireMarkerAttribute: true`) and mark only the functions that are safe to log with `[LogFunctionInputs]`, `[LogFunctionOutputs]` or `[LogFunctionInputsAndOutputs]`.
Unmarked functions are not logged at all.
