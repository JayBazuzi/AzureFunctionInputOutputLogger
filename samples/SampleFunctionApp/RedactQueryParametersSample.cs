using Bazuzi.AzureFunctionInputOutputLogger;
using Microsoft.Azure.Functions.Worker;

namespace SampleFunctionApp;

// Not used by the sample app; exists so the README snippet is compiled.
internal static class RedactQueryParametersSample
{
    public static void Register(IFunctionsWorkerApplicationBuilder worker)
    {
        // begin-snippet: redact-query-parameters
        worker.UseInputOutputLogging(options =>
        {
            options.RequireMarkerAttribute = true;
            options.RedactedQueryParameters.Add("access_token");
            options.RedactedQueryParameters.Add("sig");
        });
        // end-snippet
    }
}
