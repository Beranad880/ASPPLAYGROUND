using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApplicationASP01.Hubs;
using WebApplicationASP01.Models;
using WebApplicationASP01.Services;

namespace WebApplicationASP01.App;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LinksController : ControllerBase
{
    private readonly LinkService _linkService;
    private readonly IHubContext<LinkHub> _hubContext;
    private readonly ILogger<LinksController> _logger;

    public LinksController(LinkService linkService, IHubContext<LinkHub> hubContext, ILogger<LinksController> logger)
    {
        _linkService = linkService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Vrátí seznam všech uložených textů/URL z Redis seřazených od nejnovějšího.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<LinkEntry>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LinkEntry>>> GetAll()
    {
        var links = await _linkService.GetAllAsync();
        return Ok(links);
    }

    /// <summary>
    /// Uloží nový text nebo URL odkaz do Redis listu (LPUSH + LTRIM na 50 + TTL 7 dní).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LinkEntry), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LinkEntry>> Create([FromBody] CreateLinkDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return BadRequest(new { message = "Obsah (content/text/url) nesmí být prázdný." });
        }

        var entry = await _linkService.CreateAsync(dto.Content);
        await _hubContext.Clients.All.SendAsync("LinksUpdated");
        return CreatedAtAction(nameof(GetAll), new { id = entry.Id }, entry);
    }

    /// <summary>
    /// Vrátí stav Redis připojení a počet uložených odkazů.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(LinkServiceStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<LinkServiceStatus>> GetStatus()
    {
        var status = await _linkService.GetStatusAsync();
        return Ok(status);
    }

    /// <summary>
    /// Smaže všechny uložené záznamy z Redis (smazání klíče "shared:links").
    /// </summary>
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearAll()
    {
        await _linkService.ClearAllAsync();
        await _hubContext.Clients.All.SendAsync("LinksUpdated");
        return Ok(new { message = "Všechny sdílené odkazy byly úspěšně smazány." });
    }

    /// <summary>
    /// Smaže konkrétní text/URL podle ID (GUID) nebo indexu v seznamu.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _linkService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Odkaz s identifikátorem nebo indexem '{id}' nebyl nalezen." });
        }

        await _hubContext.Clients.All.SendAsync("LinksUpdated");
        return NoContent();
    }
}
