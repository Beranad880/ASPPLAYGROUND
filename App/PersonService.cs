using Microsoft.EntityFrameworkCore;

namespace WebApplicationASP01.App;

public class PersonService
{
    private readonly AppDbContext _context;

    public PersonService(AppDbContext context)
    {
        _context = context;
    }

    public string GetGreeting()
    {
        return "Hello from PersonService!";
    }

    public async Task<List<Person>> GetAllAsync()
    {
        return await _context.Persons.AsNoTracking().OrderByDescending(p => p.Id).ToListAsync();
    }

    public async Task<WebApplicationASP01.Models.PagedResult<Person>> GetPagedAsync(int page = 1, int pageSize = 50)
    {
        var query = _context.Persons.AsNoTracking();
        var total = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new WebApplicationASP01.Models.PagedResult<Person>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Person?> GetByIdAsync(Guid id)
    {
        return await _context.Persons.FindAsync(id);
    }

    public async Task<Person> CreateAsync(CreatePersonDto dto)
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            Jmeno = dto.Jmeno,
            DatumNarozeni = dto.DatumNarozeni,
            TrvalaAdresa = dto.TrvalaAdresa,
            RodneCislo = dto.RodneCislo,
            Telefon = dto.Telefon,
            Email = dto.Email
        };

        _context.Persons.Add(person);
        await _context.SaveChangesAsync();
        return person;
    }

    public async Task<Person?> UpdateAsync(Guid id, UpdatePersonDto dto)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null)
        {
            return null;
        }

        person.Jmeno = dto.Jmeno;
        person.DatumNarozeni = dto.DatumNarozeni;
        person.TrvalaAdresa = dto.TrvalaAdresa;
        person.RodneCislo = dto.RodneCislo;
        person.Telefon = dto.Telefon;
        person.Email = dto.Email;

        await _context.SaveChangesAsync();
        return person;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null)
        {
            return false;
        }

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync();
        return true;
    }
}
