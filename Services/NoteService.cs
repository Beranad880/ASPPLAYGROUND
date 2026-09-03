using WebApplicationASP01.Data;
using WebApplicationASP01.Models;
using Microsoft.AspNetCore.SignalR;
using WebApplicationASP01.Hubs;

namespace WebApplicationASP01.Services;

public class NoteService
{
    private readonly INoteRepository _repository;
    private readonly IHubContext<NotesHub> _hubContext;

    public NoteService(INoteRepository repository, IHubContext<NotesHub> hubContext)
    {
        _repository = repository;
        _hubContext = hubContext;
    }

    public Task<List<Note>> GetAllAsync() => _repository.GetAllAsync();

    public Task<Note?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public Task<List<Note>> SearchAsync(string query) => _repository.SearchAsync(query);

    public async Task<Note> CreateAsync(CreateNoteDto dto)
    {
        var note = new Note
        {
            Title = dto.Title,
            Content = dto.Content,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.CreateAsync(note);
        await _hubContext.Clients.All.SendAsync("NoteCreated", created);
        return created;
    }

    public async Task<Note?> UpdateAsync(Guid id, UpdateNoteDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        existing.Title = dto.Title;
        existing.Content = dto.Content;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        await _hubContext.Clients.All.SendAsync("NoteUpdated", updated);
        return updated;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
        {
            await _hubContext.Clients.All.SendAsync("NoteDeleted", id);
        }
        return deleted;
    }
}
