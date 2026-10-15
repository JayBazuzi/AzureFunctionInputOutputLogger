using Bazuzi.AzureFunctionInputOutputLogger;
using Microsoft.Extensions.Hosting;

// begin-snippet: register-middleware
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => worker.UseInputOutputLogging(requireMarkerAttribute: false))
    .Build();

host.Run();
// end-snippet
