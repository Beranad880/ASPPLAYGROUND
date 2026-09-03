using System.Collections.Concurrent;
using System.Text.Json;
using StackExchange.Redis;
using WebApplicationASP01.Models;

namespace WebApplicationASP01.Services;

/// <summary>
/// Služba pro správu sdílených textů/URL v Redis listu ("shared:links") s fallbackem a limitem 50 položek.
/// </summary>
public class LinkService
{
    public const string DefaultRedisKey = "shared:links";
    public const int MaxItems = 50;
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromDays(7);

    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<LinkService> _logger;

    // In-memory fallback pro situace, kdy Redis není dostupný
    private readonly List<LinkEntry> _inMemoryFallback = new();
    private readonly object _lock = new();

    public LinkService(ILogger<LinkService> logger, IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _redis = redis;
    }

    /// <summary>
    /// Ověří, zda je Redis aktivní a připojený.
    /// </summary>
    public bool IsRedisAvailable()
    {
        try
        {
            return _redis != null && _redis.IsConnected;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Získá všechny uložené texty/URL seřazené od nejnovějšího.
    /// </summary>
    public async Task<List<LinkEntry>> GetAllAsync()
    {
        if (IsRedisAvailable())
        {
            try
            {
                var db = _redis!.GetDatabase();
                var rawItems = await db.ListRangeAsync(DefaultRedisKey, 0, MaxItems - 1);

                var list = new List<LinkEntry>();
                foreach (var rawItem in rawItems)
                {
                    if (rawItem.IsNullOrEmpty) continue;

                    try
                    {
                        var entry = JsonSerializer.Deserialize<LinkEntry>(rawItem.ToString());
                        if (entry != null)
                        {
                            list.Add(entry);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Chyba při deserializaci položky z Redis listu.");
                    }
                }

                // Synchronizace do lokální in-memory paměti pro případ výpadku
                lock (_lock)
                {
                    _inMemoryFallback.Clear();
                    _inMemoryFallback.AddRange(list);
                }

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Výpadek čtení z Redis. Používám in-memory zálohu.");
            }
        }

        // Fallback: In-memory kopie
        lock (_lock)
        {
            return _inMemoryFallback.Take(MaxItems).ToList();
        }
    }

    /// <summary>
    /// Vloží nový text nebo URL odkaz na začátek seznamu v Redis (LPUSH), zkrátí na 50 (LTRIM) a nastaví TTL 7 dní (EXPIRE).
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

        var json = JsonSerializer.Serialize(entry);

        if (IsRedisAvailable())
        {
            try
            {
                var db = _redis!.GetDatabase();

                // 1. Vložení na začátek listu
                await db.ListLeftPushAsync(DefaultRedisKey, json);

                // 2. Omezení na posledních MaxItems (50) položek
                await db.ListTrimAsync(DefaultRedisKey, 0, MaxItems - 1);

                // 3. Nastavení expirace 7 dní
                await db.KeyExpireAsync(DefaultRedisKey, DefaultTtl);

                _logger.LogInformation("Nový odkaz (ID: {Id}, IsUrl: {IsUrl}) úspěšně uložen do Redis.", entry.Id, entry.IsUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nepodařilo se zapsat do Redis. Ukládám do in-memory paměti.");
            }
        }

        // Vždy uložíme i do in-memory fallbacku
        lock (_lock)
        {
            _inMemoryFallback.Insert(0, entry);
            if (_inMemoryFallback.Count > MaxItems)
            {
                _inMemoryFallback.RemoveRange(MaxItems, _inMemoryFallback.Count - MaxItems);
            }
        }

        return entry;
    }

    /// <summary>
    /// Smaže položku podle ID (GUID) nebo číselného indexu.
    /// </summary>
    public async Task<bool> DeleteAsync(string idOrIndex)
    {
        if (string.IsNullOrWhiteSpace(idOrIndex))
        {
            return false;
        }

        var deletedFromRedis = false;

        if (IsRedisAvailable())
        {
            try
            {
                var db = _redis!.GetDatabase();
                var rawItems = await db.ListRangeAsync(DefaultRedisKey, 0, -1);

                RedisValue? itemToDelete = null;

                // 1. Zkusíme najít podle ID v JSONu
                foreach (var raw in rawItems)
                {
                    if (raw.IsNullOrEmpty) continue;
                    try
                    {
                        var entry = JsonSerializer.Deserialize<LinkEntry>(raw.ToString());
                        if (entry != null && string.Equals(entry.Id, idOrIndex, StringComparison.OrdinalIgnoreCase))
                        {
                            itemToDelete = raw;
                            break;
                        }
                    }
                    catch { }
                }

                // 2. Pokud nebylo nalezeno podle ID a idOrIndex je číslo, zkusíme index
                if (itemToDelete == null && int.TryParse(idOrIndex, out var idx) && idx >= 0 && idx < rawItems.Length)
                {
                    itemToDelete = rawItems[idx];
                }

                if (itemToDelete.HasValue)
                {
                    var removedCount = await db.ListRemoveAsync(DefaultRedisKey, itemToDelete.Value, 1);
                    deletedFromRedis = removedCount > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chyba při mazání z Redis.");
            }
        }

        // Smazání i z in-memory paměti
        var deletedFromMemory = false;
        lock (_lock)
        {
            var targetIndex = _inMemoryFallback.FindIndex(e => string.Equals(e.Id, idOrIndex, StringComparison.OrdinalIgnoreCase));
            if (targetIndex >= 0)
            {
                _inMemoryFallback.RemoveAt(targetIndex);
                deletedFromMemory = true;
            }
            else if (int.TryParse(idOrIndex, out var numIdx) && numIdx >= 0 && numIdx < _inMemoryFallback.Count)
            {
                _inMemoryFallback.RemoveAt(numIdx);
                deletedFromMemory = true;
            }
        }

        return deletedFromRedis || deletedFromMemory;
    }

    /// <summary>
    /// Smaže celý seznam v Redis a vyčistí in-memory paměť.
    /// </summary>
    public async Task<bool> ClearAllAsync()
    {
        var redisSuccess = false;
        if (IsRedisAvailable())
        {
            try
            {
                var db = _redis!.GetDatabase();
                await db.KeyDeleteAsync(DefaultRedisKey);
                redisSuccess = true;
                _logger.LogInformation("Klíč Redis '{Key}' byl kompletně smazán.", DefaultRedisKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nepodařilo se smazat klíč z Redis.");
            }
        }

        lock (_lock)
        {
            _inMemoryFallback.Clear();
        }

        return redisSuccess || true;
    }

    /// <summary>
    /// Vrátí stav Redis připojení a počet uložených položek.
    /// </summary>
    public async Task<LinkServiceStatus> GetStatusAsync()
    {
        var isConnected = IsRedisAvailable();
        var items = await GetAllAsync();

        return new LinkServiceStatus
        {
            IsRedisConnected = isConnected,
            StorageType = isConnected ? "Redis" : "InMemoryFallback",
            Count = items.Count,
            RedisKey = DefaultRedisKey,
            MaxLimit = MaxItems,
            TtlDays = (int)DefaultTtl.TotalDays,
            Message = isConnected
                ? "Redis je připojen a funkční."
                : "Redis není dostupný. Aplikace běží v in-memory fallback režimu."
        };
    }
}
