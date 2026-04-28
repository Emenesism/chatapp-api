using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers.Healthz;

[ApiController]
[Route("healthz")]

public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "ok",
            status = "The server is Up."
        });
    }
}
