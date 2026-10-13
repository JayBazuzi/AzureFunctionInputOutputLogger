using AzureFunctionInputOutputLogger;
using Microsoft.Extensions.Hosting;

// begin-snippet: register-middleware
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => worker.UseInputOutputLogging())
    .Build();

host.Run();
// end-snippet
