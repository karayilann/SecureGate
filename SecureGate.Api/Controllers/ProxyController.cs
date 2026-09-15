using Microsoft.AspNetCore.Mvc;

namespace SecureGate.Api.Controllers;

[ApiController]
[Route("proxy")]
public class ProxyController : ControllerBase
{
    [HttpGet("optimize")]
    public IActionResult Optimize([FromQuery] string url)
    {
        return Ok(new { message = "Middleware passed", url });
    }
}