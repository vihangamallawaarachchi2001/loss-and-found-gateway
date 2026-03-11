using Microsoft.AspNetCore.Mvc;

namespace gateway.Controllers;

[ApiController]
public sealed class HealthController : ControllerBase
{
    [HttpGet("/healthz")]
    public IActionResult Healthz() => Ok(new { status = "ok" });
}
