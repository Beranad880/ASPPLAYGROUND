using System.Collections.Concurrent;
using WebApplicationASP01.Models;

namespace WebApplicationASP01.Services;

public class ChatHistoryService
{
    private readonly ConcurrentQueue<ChatMessage> _messages = new();
    private const int MaxHistory = 100;

    public void AddMessage(ChatMessage message)
    {
        _messages.Enqueue(message);
        while (_messages.Count > MaxHistory && _messages.TryDequeue(out _))
        {
        }
    }

    public IReadOnlyList<ChatMessage> GetRecentMessages()
    {
        return _messages.ToArray();
    }
}
