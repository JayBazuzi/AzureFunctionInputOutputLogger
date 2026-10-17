using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bazuzi.AzureFunctionInputOutputLogger;

public static class FunctionsWorkerApplicationBuilderExtensions
{
    /// <summary>
    /// Logs HTTP requests and responses for apps that use the built-in HTTP model.
    /// </summary>
    public static TBuilder UseInputOutputLogging<TBuilder>(
        this TBuilder builder,
        bool requireMarkerAttribute)
        where TBuilder : IFunctionsWorkerApplicationBuilder
        => builder.UseInputOutputLogging(options => options.RequireMarkerAttribute = requireMarkerAttribute);

    /// <summary>
    /// Logs HTTP requests and responses for apps that use the built-in HTTP model.
    /// </summary>
    public static TBuilder UseInputOutputLogging<TBuilder>(
        this TBuilder builder,
        Action<InputOutputLoggingOptions> configure)
        where TBuilder : IFunctionsWorkerApplicationBuilder
    {
        var options = new InputOutputLoggingOptions();
        configure(options);
        var requireMarkerAttribute = options.RequireMarkerAttribute;
        var queryFormatter = new QueryFormatter(options.RedactedQueryParameters);

        builder.Use(
            next => context =>
            {
                var logger = context.InstanceServices.GetRequiredService<ILogger<InputOutputLoggingMiddleware>>();
                return new InputOutputLoggingMiddleware(logger, requireMarkerAttribute, queryFormatter).Invoke(context, next);
            });

        return builder;
    }
}
