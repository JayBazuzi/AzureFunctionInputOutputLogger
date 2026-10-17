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
    public static TBuilder UseAspNetCoreInputOutputLogging<TBuilder>(
        this TBuilder builder,
        bool requireMarkerAttribute)
        where TBuilder : IFunctionsWorkerApplicationBuilder
        => builder.UseAspNetCoreInputOutputLogging(options => options.RequireMarkerAttribute = requireMarkerAttribute);

    /// <summary>
    /// Logs HTTP requests and responses for apps that call <c>ConfigureFunctionsWebApplication</c>.
    /// Call it after <c>ConfigureFunctionsWebApplication</c>.
    /// </summary>
    public static TBuilder UseAspNetCoreInputOutputLogging<TBuilder>(
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
                return new AspNetCoreInputOutputLoggingMiddleware(logger, requireMarkerAttribute, queryFormatter).Invoke(context, next);
            });

        return builder;
    }
}
