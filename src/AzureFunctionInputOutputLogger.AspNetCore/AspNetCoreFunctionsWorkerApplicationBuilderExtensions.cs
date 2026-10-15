using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureFunctionInputOutputLogger;

public static class AspNetCoreFunctionsWorkerApplicationBuilderExtensions
{
    /// <summary>
    /// Logs HTTP requests and responses for apps that call <c>ConfigureFunctionsWebApplication</c>.
    /// Call it after <c>ConfigureFunctionsWebApplication</c>.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseAspNetCoreInputOutputLogging(
        this IFunctionsWorkerApplicationBuilder builder,
        bool requireMarkerAttribute)
    {
        return builder.Use(
            next => context =>
            {
                var logger = context.InstanceServices.GetRequiredService<ILogger<InputOutputLoggingMiddleware>>();
                return new AspNetCoreInputOutputLoggingMiddleware(logger, requireMarkerAttribute).Invoke(context, next);
            });
    }
}
