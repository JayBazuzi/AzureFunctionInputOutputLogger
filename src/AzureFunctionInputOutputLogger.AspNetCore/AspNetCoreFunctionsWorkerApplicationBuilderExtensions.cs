using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bazuzi.AzureFunctionInputOutputLogger;

public static class AspNetCoreFunctionsWorkerApplicationBuilderExtensions
{
    /// <summary>
    /// Logs HTTP requests and responses for apps that call <c>ConfigureFunctionsWebApplication</c>.
    /// Call it after <c>ConfigureFunctionsWebApplication</c>.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseAspNetCoreInputOutputLogging(
        this IFunctionsWorkerApplicationBuilder builder,
        bool requireMarkerAttribute)
        => builder.UseAspNetCoreInputOutputLogging(options => options.RequireMarkerAttribute = requireMarkerAttribute);

    /// <summary>
    /// Logs HTTP requests and responses for apps that call <c>ConfigureFunctionsWebApplication</c>.
    /// Call it after <c>ConfigureFunctionsWebApplication</c>.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseAspNetCoreInputOutputLogging(
        this IFunctionsWorkerApplicationBuilder builder,
        Action<InputOutputLoggingOptions> configure)
    {
        var options = new InputOutputLoggingOptions();
        configure(options);
        var requireMarkerAttribute = options.RequireMarkerAttribute;
        var queryFormatter = new QueryFormatter(options.RedactedQueryParameters);

        return builder.Use(
            next => context =>
            {
                var logger = context.InstanceServices.GetRequiredService<ILogger<InputOutputLoggingMiddleware>>();
                return new AspNetCoreInputOutputLoggingMiddleware(logger, requireMarkerAttribute, queryFormatter).Invoke(context, next);
            });
    }
}
