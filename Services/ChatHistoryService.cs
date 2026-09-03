using System.Collections.Concurrent;
using System.Text.Json;
using StackExchange.Redis;
using WebApplicationASP01.Models;

namespace WebApplicationASP01.Services;

/// <summary>
/// Služba pro perzistenci historie globálního SignalR chatu v Redis listu ("global:chat:messages") s in-memory fallbackem.
/// </summary>
public class ChatHistoryService
{
    public const string DefaultRedisKey = "global:chat:messages";
    public const int MaxHistory = 100;
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromDays(7);

    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<ChatHistoryService> _logger;

    // In-memory fallback fronta
    private readonly ConcurrentQueue<ChatMessage> _inMemoryQueue = new();

    public ChatHistoryService(ILogger<ChatHistoryService> logger, IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _redis = redis;
    }

    /// <summary>
    /// Ověří, zda je Redis připojený a dostupný.
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
    /// Asynchronně uloží zprávu do Redis listu (RPUSH) s limitem 100 posledních zpráv (LTRIM -100 -1) a TTL 7 dní.
    /// </summary>
    public async Task AddMessageAsync(ChatMessage message)
    {
        var json = JsonSerializer.Serialize(message);

        if (IsRedisAvailable())
        {
            try
            {
                var db = _redis!.GetDatabase();

                // 1. Přidání zprávy na konec fronty v Redis
                await db.ListRightPushAsync(DefaultRedisKey, json);

                // 2. Omezení na posledních MaxHistory (100) zpráv
                await db.ListTrimAsync(DefaultRedisKey, -MaxHistory, -1);

                // 3. Prodloužení expirace na 7 dní
                await db.KeyExpireAsync(DefaultRedisKey, DefaultTtl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nepodařilo se uložit chat zprávu do Redis. Ukládám do in-memory zálohy.");
            }
        }

        // Vždy uložíme i do in-memory fronty pro okamžitý fallback
        _inMemoryQueue.Enqueue(message);
        while (_inMemoryQueue.Count > MaxHistory && _inMemoryQueue.TryDequeue(out _))
        {
        }
    }

    /// <summary>
    /// Synchronní verze pro zpětnou kompatibilitu.
    /// </summary>
    public void AddMessage(ChatMessage message)
    {
        _inMemoryQueue.Enqueue(message);
        while (_inMemoryQueue.Count > MaxHistory && _inMemoryQueue.TryDequeue(out _))
        {
        }

        if (IsRedisAvailable())
        {
            Task.Run(async () =>
            {
                try
                {
                    var json = JsonSerializer.Serialize(message);
                    var db = _redis!.GetDatabase();
                    await db.ListRightPushAsync(DefaultRedisKey, json);
                    await db.ListTrimAsync(DefaultRedisKey, -MaxHistory, -1);
                    await db.KeyExpireAsync(DefaultRedisKey, DefaultTtl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Chyba při asynchronním zápisu chat zprávy do Redis.");
                }
            });
        }
    }

    /// <summary>
    /// Asynchronně načte historii posledních zpráv z Redis listu seřazenou chronologicky.
    /// </summary>
    public async Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync()
    {
        if (IsRedisAvailable())
        {
            try
            {
                var db = _redis!.GetDatabase();
                var rawItems = await db.ListRangeAsync(DefaultRedisKey, 0, -1);

                var list = new List<ChatMessage>();
                foreach (var raw in rawItems)
                {
                    if (raw.IsNullOrEmpty) continue;
                    try
                    {
                        var msg = JsonSerializer.Deserialize<ChatMessage>(raw.ToString());
                        if (msg != null)
                        {
                            list.Add(msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Chyba při deserializaci chatové zprávy z Redis listu.");
                    }
                }

                // Synchronizace in-memory zálohy
                _inMemoryQueue.Clear();
                foreach (var msg in list)
                {
                    _inMemoryQueue.Enqueue(msg);
                }

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Výpadek čtení chatové historie z Redis. Používám in-memory zálohu.");
            }
        }

        return _inMemoryQueue.ToArray();
    }

    /// <summary>
    /// Synchronní načtení historie pro PageModel nebo zobrazení.
    /// </summary>
    public IReadOnlyList<ChatMessage> GetRecentMessages()
    {
        if (IsRedisAvailable())
        {
            try
            {
                var db = _redis!.GetDatabase();
                var rawItems = db.ListRange(DefaultRedisKey, 0, -1);

                var list = new List<ChatMessage>();
                foreach (var raw in rawItems)
                {
                    if (raw.IsNullOrEmpty) continue;
                    try
                    {
                        var msg = JsonSerializer.Deserialize<ChatMessage>(raw.ToString());
                        if (msg != null)
                        {
                            list.Add(msg);
                        }
                    }
                    catch { }
                }

                if (list.Count > 0)
                {
                    return list;
                }
            }
            catch { }
        }

        return _inMemoryQueue.ToArray();
    }

    /// <summary>
    /// Asynchronně smaže celou historii chatu v Redis i in-memory paměti.
    /// </summary>
    public async Task ClearMessagesAsync()
    {
        if (IsRedisAvailable())
        {
            try
            {
                var db = _redis!.GetDatabase();
                await db.KeyDeleteAsync(DefaultRedisKey);
                _logger.LogInformation("Klíč Redis '{Key}' pro chat historii byl smazán.", DefaultRedisKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chyba při mazání chatové historie z Redis.");
            }
        }

        _inMemoryQueue.Clear();
    }

    /// <summary>
    /// Synchronní smazání historie.
    /// </summary>
    public void ClearMessages()
    {
        _inMemoryQueue.Clear();
        if (IsRedisAvailable())
        {
            try
            {
                var db = _redis!.GetDatabase();
                db.KeyDelete(DefaultRedisKey);
            }
            catch { }
        }
    }
}
