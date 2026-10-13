using System.Net;
using AzureFunctionInputOutputLogger;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace SampleFunctionApp;

public class MarkedEchoFunction
{
    // begin-snippet: marker-attribute-example
    [Function("MyFunction")]
    [LogInputOutput]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request)
    {
    // end-snippet
        var body = await new StreamReader(request.Body).ReadToEndAsync();

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"You said: {body}");
        return response;
    }
}
