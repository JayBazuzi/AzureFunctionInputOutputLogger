using AzureFunctionInputOutputLogger;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => worker.UseInputOutputLogging())
    .Build();

host.Run();
