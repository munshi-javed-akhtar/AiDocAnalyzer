using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("health")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    private readonly IVectorDbService _vectorDbService;

    public HealthController(IVectorDbService vectorDbService)
    {
        _vectorDbService = vectorDbService;
    }

    /// <summary>Check overall API health</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });

    /// <summary>Check Qdrant vector database connectivity</summary>
    [HttpGet("vector")]
    public async Task<IActionResult> GetVectorHealth()
    {
        var isHealthy = await _vectorDbService.IsHealthyAsync();
        if (isHealthy)
            return Ok(new { status = "healthy", service = "qdrant", timestamp = DateTime.UtcNow });

        return StatusCode(503, new { status = "unhealthy", service = "qdrant", timestamp = DateTime.UtcNow });
    }
}
