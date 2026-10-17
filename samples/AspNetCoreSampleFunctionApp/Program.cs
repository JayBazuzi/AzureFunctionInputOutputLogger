using Bazuzi.AzureFunctionInputOutputLogger;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

// begin-snippet: register-aspnetcore-middleware
FunctionsApplication.CreateBuilder(args)
    .ConfigureFunctionsWebApplication()
    .UseAspNetCoreInputOutputLogging(requireMarkerAttribute: false)
    .Build()
    .Run();
// end-snippet
