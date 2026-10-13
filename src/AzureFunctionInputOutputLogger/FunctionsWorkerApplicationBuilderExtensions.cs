using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

namespace AzureFunctionInputOutputLogger;

public static class FunctionsWorkerApplicationBuilderExtensions
{
    /// <summary>
    /// Logs the query parameters, request body and response body of every HTTP-triggered function.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseInputOutputLogging(this IFunctionsWorkerApplicationBuilder builder) =>
        builder.UseMiddleware<InputOutputLoggingMiddleware>();
}
