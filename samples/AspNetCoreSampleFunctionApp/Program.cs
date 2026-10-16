using Bazuzi.AzureFunctionInputOutputLogger;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

// begin-snippet: register-aspnetcore-middleware
var builder = FunctionsApplication.CreateBuilder(args).ConfigureFunctionsWebApplication();
builder.UseAspNetCoreInputOutputLogging(requireMarkerAttribute: false);

builder.Build().Run();
// end-snippet
