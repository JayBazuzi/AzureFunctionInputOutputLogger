using Bazuzi.AzureFunctionInputOutputLogger;
using Microsoft.Azure.Functions.Worker;

namespace AspNetCoreSampleFunctionApp;

// Not used by the sample app; exists so the README snippet is compiled.
internal static class RequireMarkerAttributeSample
{
    public static void Register(IFunctionsWorkerApplicationBuilder worker)
    {
        // begin-snippet: aspnetcore-require-marker-attribute
        worker.UseAspNetCoreInputOutputLogging(requireMarkerAttribute: true);
        // end-snippet
    }
}
