using Bazuzi.AzureFunctionInputOutputLogger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AspNetCoreSampleFunctionApp;

public class MarkedEchoFunction
{
    // begin-snippet: aspnetcore-marker-attribute-example
    [Function("MyFunction")]
    [LogFunctionInputsAndOutputs]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest request)
    {
        // end-snippet
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        return new OkObjectResult(new { youSaid = body });
    }
}
