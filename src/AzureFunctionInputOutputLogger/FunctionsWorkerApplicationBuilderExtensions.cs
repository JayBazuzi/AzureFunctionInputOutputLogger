using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bazuzi.AzureFunctionInputOutputLogger;

public static class FunctionsWorkerApplicationBuilderExtensions
{
    /// <summary>
    /// Logs HTTP requests and responses for apps that use the built-in HTTP model.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseInputOutputLogging(
        this IFunctionsWorkerApplicationBuilder builder,
        bool requireMarkerAttribute)
        => builder.UseInputOutputLogging(options => options.RequireMarkerAttribute = requireMarkerAttribute);

    /// <summary>
    /// Logs HTTP requests and responses for apps that use the built-in HTTP model.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseInputOutputLogging(
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
                return new InputOutputLoggingMiddleware(logger, requireMarkerAttribute, queryFormatter).Invoke(context, next);
            });
    }
}
