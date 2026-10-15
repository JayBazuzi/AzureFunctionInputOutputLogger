using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureFunctionInputOutputLogger;

public static class FunctionsWorkerApplicationBuilderExtensions
{
    /// <summary>
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseInputOutputLogging(
        this IFunctionsWorkerApplicationBuilder builder,
        bool requireMarkerAttribute)
    {
        return builder.Use(
            next => context =>
            {
                var logger = context.InstanceServices.GetRequiredService<ILogger<InputOutputLoggingMiddleware>>();
                return new InputOutputLoggingMiddleware(logger, requireMarkerAttribute).Invoke(context, next);
            });
    }
}
