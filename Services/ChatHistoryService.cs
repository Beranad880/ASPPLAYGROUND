using System.Collections.Concurrent;
using WebApplicationASP01.Models;

namespace WebApplicationASP01.Services;

/// <summary>
/// Služba pro perzistenci historie globálního SignalR chatu v IMemoryCache s limitem 100 zpráv a TTL 7 dní.
/// </summary>
public class ChatHistoryService
{
    public const int MaxHistory = 100;

    private readonly ILogger<ChatHistoryService> _logger;

    // Thread-safe in-memory fronta zpráv
    private readonly ConcurrentQueue<ChatMessage> _queue = new();

    public ChatHistoryService(ILogger<ChatHistoryService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Uloží zprávu do in-memory fronty s limitem MaxHistory (100) zpráv.
    /// </summary>
    public Task AddMessageAsync(ChatMessage message)
    {
        Enqueue(message);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Synchronní verze pro zpětnou kompatibilitu.
    /// </summary>
    public void AddMessage(ChatMessage message)
    {
        Enqueue(message);
    }

    private void Enqueue(ChatMessage message)
    {
        _queue.Enqueue(message);
        while (_queue.Count > MaxHistory && _queue.TryDequeue(out _))
        {
        }
    }

    /// <summary>
    /// Načte historii posledních zpráv seřazenou chronologicky.
    /// </summary>
    public Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync()
    {
        IReadOnlyList<ChatMessage> result = _queue.ToArray();
        return Task.FromResult(result);
    }

    /// <summary>
    /// Synchronní načtení historie pro PageModel nebo zobrazení.
    /// </summary>
    public IReadOnlyList<ChatMessage> GetRecentMessages()
    {
        return _queue.ToArray();
    }

    /// <summary>
    /// Smaže celou historii chatu z in-memory paměti.
    /// </summary>
    public Task ClearMessagesAsync()
    {
        _queue.Clear();
        _logger.LogInformation("Chatová historie byla smazána z paměti.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Synchronní smazání historie.
    /// </summary>
    public void ClearMessages()
    {
        _queue.Clear();
    }
}
