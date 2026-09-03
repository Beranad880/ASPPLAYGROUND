using WebApplicationASP01.Models;

namespace WebApplicationASP01.Data;

public interface INoteRepository
{
    Task<List<Note>> GetAllAsync();
    Task<Note?> GetByIdAsync(Guid id);
    Task<List<Note>> SearchAsync(string query);
    Task<Note> CreateAsync(Note note);
    Task<Note> UpdateAsync(Note note);
    Task<bool> DeleteAsync(Guid id);
}
