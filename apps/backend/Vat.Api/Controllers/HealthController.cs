using Microsoft.AspNetCore.Mvc;

namespace Vat.Api.Controllers;

[ApiController]
[Route("VAT_API/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse(
            Status: "ok",
            Service: "Vat.Api",
            Timestamp: DateTimeOffset.UtcNow));
    }
}

public sealed record HealthResponse(
    string Status,
    string Service,
    DateTimeOffset Timestamp);
