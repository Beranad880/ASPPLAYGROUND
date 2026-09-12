using Microsoft.EntityFrameworkCore;
using WebApplicationASP01.App;
using WebApplicationASP01.Models;

namespace WebApplicationASP01.Services;

/// <summary>
/// Služba pro správu sdílených textů/URL v PostgreSQL tabulce shared_links (max 50 nejnovějších).
/// </summary>
public class LinkService
{
    public const int MaxItems = 50;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LinkService> _logger;

    public LinkService(IServiceScopeFactory scopeFactory, ILogger<LinkService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    private AppDbContext CreateDb(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<AppDbContext>();

    /// <summary>
    /// Získá posledních 50 uložených textů/URL seřazených od nejnovějšího.
    /// </summary>
    public async Task<List<LinkEntry>> GetAllAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = CreateDb(scope);

        return await db.SharedLinks
            .OrderByDescending(e => e.CreatedAt)
            .Take(MaxItems)
            .ToListAsync();
    }

    /// <summary>
    /// Vloží nový text nebo URL odkaz. Pokud počet přesáhne 50, nejstarší záznamy se smažou.
    /// </summary>
    public async Task<LinkEntry> CreateAsync(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var trimmed = content.Trim();
        var entry = new LinkEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            Content = trimmed,
            CreatedAt = DateTimeOffset.UtcNow,
            IsUrl = LinkEntry.CheckIsUrl(trimmed)
        };

        using var scope = _scopeFactory.CreateScope();
        var db = CreateDb(scope);

        db.SharedLinks.Add(entry);
        await db.SaveChangesAsync();

        // Udržujeme max 50 položek — smažeme přebytečné nejstarší
        var count = await db.SharedLinks.CountAsync();
        if (count > MaxItems)
        {
            var toDelete = await db.SharedLinks
                .OrderBy(e => e.CreatedAt)
                .Take(count - MaxItems)
                .ToListAsync();

            db.SharedLinks.RemoveRange(toDelete);
            await db.SaveChangesAsync();
        }

        _logger.LogInformation("Nový odkaz (ID: {Id}, IsUrl: {IsUrl}) uložen do PostgreSQL.", entry.Id, entry.IsUrl);
        return entry;
    }

    /// <summary>
    /// Smaže položku podle ID (GUID) nebo číselného indexu v seřazeném seznamu.
    /// </summary>
    public async Task<bool> DeleteAsync(string idOrIndex)
    {
        if (string.IsNullOrWhiteSpace(idOrIndex))
            return false;

        using var scope = _scopeFactory.CreateScope();
        var db = CreateDb(scope);

        // 1. Pokus o smazání podle GUID
        var byId = await db.SharedLinks
            .FirstOrDefaultAsync(e => e.Id == idOrIndex);

        if (byId != null)
        {
            db.SharedLinks.Remove(byId);
            await db.SaveChangesAsync();
            return true;
        }

        // 2. Pokud jde o číslo, smaž podle pozice v seřazeném seznamu
        if (int.TryParse(idOrIndex, out var idx) && idx >= 0)
        {
            var byIndex = await db.SharedLinks
                .OrderByDescending(e => e.CreatedAt)
                .Skip(idx)
                .FirstOrDefaultAsync();

            if (byIndex != null)
            {
                db.SharedLinks.Remove(byIndex);
                await db.SaveChangesAsync();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Smaže všechny záznamy z tabulky shared_links.
    /// </summary>
    public async Task<bool> ClearAllAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = CreateDb(scope);

        await db.SharedLinks.ExecuteDeleteAsync();
        _logger.LogInformation("Tabulka shared_links byla kompletně vyprázdněna.");
        return true;
    }

    /// <summary>
    /// Vrátí stav úložiště a počet uložených položek.
    /// </summary>
    public async Task<LinkServiceStatus> GetStatusAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = CreateDb(scope);

        var count = await db.SharedLinks.CountAsync();

        return new LinkServiceStatus
        {
            StorageType = "PostgreSQL",
            Count = count,
            MaxLimit = MaxItems,
            Message = "Záznamy jsou persistentně uloženy v PostgreSQL tabulce shared_links."
        };
    }
}
