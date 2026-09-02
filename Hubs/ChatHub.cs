using Microsoft.AspNetCore.SignalR;
using WebApplicationASP01.Models;
using WebApplicationASP01.Services;

namespace WebApplicationASP01.Hubs;

public class ChatHub : Hub
{
    private readonly ChatHistoryService _historyService;
    private static int _connectedUsersCount = 0;

    public ChatHub(ChatHistoryService historyService)
    {
        _historyService = historyService;
    }

    public async Task SendMessage(string user, string message)
    {
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        user = user.Trim();
        message = message.Trim();

        if (message.Length > 1000)
        {
            message = message.Substring(0, 1000);
        }

        if (user.Length > 30)
        {
            user = user.Substring(0, 30);
        }

        var chatMessage = new ChatMessage(user, message, DateTime.Now);
        _historyService.AddMessage(chatMessage);

        await Clients.All.SendAsync("ReceiveMessage", new
        {
            user = chatMessage.User,
            message = chatMessage.Message,
            timestamp = chatMessage.Timestamp.ToString("HH:mm:ss"),
            isSystem = false
        });
    }

    public async Task ClearChat(string user)
    {
        if (string.IsNullOrWhiteSpace(user))
        {
            user = "Někdo";
        }
        else
        {
            user = user.Trim();
            if (user.Length > 30)
            {
                user = user.Substring(0, 30);
            }
        }

        _historyService.ClearMessages();

        await Clients.All.SendAsync("ChatCleared", user);
    }

    public override async Task OnConnectedAsync()
    {
        Interlocked.Increment(ref _connectedUsersCount);
        
        await Clients.All.SendAsync("UpdateUserCount", _connectedUsersCount);

        var history = _historyService.GetRecentMessages().Select(m => new
        {
            user = m.User,
            message = m.Message,
            timestamp = m.Timestamp.ToString("HH:mm:ss"),
            isSystem = m.IsSystem
        });

        await Clients.Caller.SendAsync("LoadHistory", history);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Interlocked.Decrement(ref _connectedUsersCount);
        await Clients.All.SendAsync("UpdateUserCount", _connectedUsersCount);

        await base.OnDisconnectedAsync(exception);
    }
}
