using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace SampleFunctionApp;

public class EchoFunction
{
    [Function("Echo")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request)
    {
        var body = await new StreamReader(request.Body).ReadToEndAsync();

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"You said: {body}");
        return response;
    }
}
