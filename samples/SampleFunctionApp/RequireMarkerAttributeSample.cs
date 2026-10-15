using Bazuzi.AzureFunctionInputOutputLogger;
using Microsoft.Azure.Functions.Worker;

namespace SampleFunctionApp;

// Not used by the sample app; exists so the README snippet is compiled.
internal static class RequireMarkerAttributeSample
{
    public static void Register(IFunctionsWorkerApplicationBuilder worker)
    {
        // begin-snippet: require-marker-attribute
        worker.UseInputOutputLogging(requireMarkerAttribute: true);
        // end-snippet
    }
}
