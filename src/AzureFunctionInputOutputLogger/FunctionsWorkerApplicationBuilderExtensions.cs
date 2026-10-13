using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzureFunctionInputOutputLogger;

public static class FunctionsWorkerApplicationBuilderExtensions
{
    /// <summary>
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseInputOutputLogging(
        this IFunctionsWorkerApplicationBuilder builder,
        bool requireMarkerAttribute = false)
    {
        builder.Services.AddSingleton(new InputOutputLoggingOptions(requireMarkerAttribute));
        return builder.UseMiddleware<InputOutputLoggingMiddleware>();
    }
}
