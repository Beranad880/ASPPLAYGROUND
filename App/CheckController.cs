using Microsoft.AspNetCore.Mvc;
using WebApplicationASP01.Models;
using WebApplicationASP01.Services;

namespace WebApplicationASP01.App;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CheckController : ControllerBase
{
    private readonly SystemCheckService _checkService;

    public CheckController(SystemCheckService checkService)
    {
        _checkService = checkService;
    }

    /// <summary>
    /// Provede kompletní kontrolu konektivity k PostgreSQL databázi a Redis serveru.
    /// </summary>
    /// <returns>Detailní stav obou služeb včetně latence, verzí a případných chyb.</returns>
    [HttpGet]
    [HttpGet("/check/status")]
    [ProducesResponseType(typeof(SystemCheckResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemCheckResponse>> GetCheck()
    {
        var result = await _checkService.PerformCheckAsync();
        return Ok(result);
    }

    /// <summary>
    /// Rychlý healthcheck ping.
    /// </summary>
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        return Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow });
    }
}
