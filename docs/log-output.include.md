## What gets logged

Each HTTP-triggered invocation produces two `Information` log entries, with these message templates:

- `Request {Method} {Path} query: {Query} body: {Body}`
- `Response {StatusCode} body: {Body}`

For example:

snippet: tests\AzureFunctionInputOutputLogger.Tests\InputOutputLoggingMiddlewareTests.LogsRequestAndResponseAsSeparateEntries.verified.txt

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
Unmarked functions are not logged at all.
