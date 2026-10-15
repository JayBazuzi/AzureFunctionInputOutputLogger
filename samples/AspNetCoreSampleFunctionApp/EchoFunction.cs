using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AspNetCoreSampleFunctionApp;

public class EchoFunction
{
    [Function("Echo")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest request)
    {
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        return new OkObjectResult(new { youSaid = body });
    }
}
