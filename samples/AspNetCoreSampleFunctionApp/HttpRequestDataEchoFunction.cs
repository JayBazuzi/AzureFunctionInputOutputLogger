using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AspNetCoreSampleFunctionApp;

// ASP.NET Core integration still allows functions that use the built-in HTTP types.
public class HttpRequestDataEchoFunction
{
    [Function("HttpRequestDataEcho")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request)
    {
        var body = await new StreamReader(request.Body).ReadToEndAsync();

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"You said: {body}");
        return response;
    }
}
