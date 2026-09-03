using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.SignalR;
using WebApplicationASP01.Hubs;

namespace WebApplicationASP01.App;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PersonsController : ControllerBase
{
    private readonly PersonService _personService;
    private readonly IHubContext<PersonHub> _hubContext;

    public PersonsController(PersonService personService, IHubContext<PersonHub> hubContext)
    {
        _personService = personService;
        _hubContext = hubContext;
    }

    [HttpGet("ahoj")]
    public IActionResult Index()
    {
        return Ok(new { message = _personService.GetGreeting() });
    }

    [HttpGet]
    public async Task<ActionResult<WebApplicationASP01.Models.PagedResult<Person>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _personService.GetPagedAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Person>> GetById(Guid id)
    {
        var person = await _personService.GetByIdAsync(id);
        if (person == null)
        {
            return NotFound(new { message = $"Osoba s ID '{id}' nebyla nalezena." });
        }

        return Ok(person);
    }

    [HttpPost]
    public async Task<ActionResult<Person>> Create([FromBody] CreatePersonDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _personService.CreateAsync(dto);
        await _hubContext.Clients.All.SendAsync("PersonsUpdated");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Person>> Update(Guid id, [FromBody] UpdatePersonDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _personService.UpdateAsync(id, dto);
        if (updated == null)
        {
            return NotFound(new { message = $"Osoba s ID '{id}' nebyla nalezena." });
        }

        await _hubContext.Clients.All.SendAsync("PersonsUpdated");
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _personService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Osoba s ID '{id}' nebyla nalezena." });
        }

        await _hubContext.Clients.All.SendAsync("PersonsUpdated");
        return NoContent();
    }
}
