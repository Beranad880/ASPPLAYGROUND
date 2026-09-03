using Microsoft.EntityFrameworkCore;
using WebApplicationASP01.App;
using WebApplicationASP01.Models;

namespace WebApplicationASP01.Data;

public class NoteRepository : INoteRepository
{
    private readonly AppDbContext _context;

    public NoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Note>> GetAllAsync()
    {
        return await _context.Notes.OrderByDescending(n => n.UpdatedAt).ToListAsync();
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        return await _context.Notes.FindAsync(id);
    }

    public async Task<List<Note>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<Note>();

        // EF Core Translate this to ILIKE in PostgreSQL
        return await _context.Notes
            .Where(n => EF.Functions.ILike(n.Title, $"%{query}%") || EF.Functions.ILike(n.Content, $"%{query}%"))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Note> CreateAsync(Note note)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<Note> UpdateAsync(Note note)
    {
        _context.Notes.Update(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null)
            return false;

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();
        return true;
    }
}
