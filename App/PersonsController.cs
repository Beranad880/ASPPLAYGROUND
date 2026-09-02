using Microsoft.AspNetCore.Mvc;

namespace WebApplicationASP01.App;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PersonsController : ControllerBase
{
    private readonly PersonService _personService;

    public PersonsController(PersonService personService)
    {
        _personService = personService;
    }

    [HttpGet("ahoj")]
    public IActionResult Index()
    {
        return Ok(new { message = _personService.GetGreeting() });
    }

    [HttpGet]
    public async Task<ActionResult<List<Person>>> GetAll()
    {
        var list = await _personService.GetAllAsync();
        return Ok(list);
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

        return NoContent();
    }
}
