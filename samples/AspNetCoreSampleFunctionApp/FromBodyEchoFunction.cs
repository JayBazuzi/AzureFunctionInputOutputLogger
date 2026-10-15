using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace AspNetCoreSampleFunctionApp;

// The body is bound after the logging middleware reads it, so this checks that logging leaves the body readable.
public class FromBodyEchoFunction
{
    [Function("FromBodyEcho")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
        [FromBody] EchoRequest body)
    {
        return new OkObjectResult(new { youSaid = body.Message });
    }
}

public record EchoRequest(string Message);
