using Microsoft.AspNetCore.Mvc;
using WebApplicationASP01.Models;
using WebApplicationASP01.Services;

namespace WebApplicationASP01.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NotesController : ControllerBase
{
    private readonly NoteService _noteService;

    public NotesController(NoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Note>>> GetAll()
    {
        var notes = await _noteService.GetAllAsync();
        return Ok(notes);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Note>> GetById(Guid id)
    {
        var note = await _noteService.GetByIdAsync(id);
        if (note == null) return NotFound(new { message = "Poznámka nenalezena." });
        return Ok(note);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<Note>>> Search([FromQuery] string query)
    {
        var notes = await _noteService.SearchAsync(query);
        return Ok(notes);
    }

    [HttpPost]
    public async Task<ActionResult<Note>> Create([FromBody] CreateNoteDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _noteService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Note>> Update(Guid id, [FromBody] UpdateNoteDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _noteService.UpdateAsync(id, dto);
        if (updated == null) return NotFound(new { message = "Poznámka nenalezena." });

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _noteService.DeleteAsync(id);
        if (!deleted) return NotFound(new { message = "Poznámka nenalezena." });

        return NoContent();
    }
}
