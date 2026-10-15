using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AspNetCoreSampleFunctionApp;

public class NotFoundFunction
{
    [Function("NotFound")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request)
        => new NotFoundResult();
}
