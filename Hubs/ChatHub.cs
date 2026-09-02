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

        var clientIp = GetClientIpAddress();
        var chatMessage = new ChatMessage(user, message, DateTime.Now, false, clientIp);
        _historyService.AddMessage(chatMessage);

        await Clients.All.SendAsync("ReceiveMessage", new
        {
            user = chatMessage.User,
            message = chatMessage.Message,
            timestamp = chatMessage.Timestamp.ToString("HH:mm:ss"),
            isSystem = false,
            ipAddress = chatMessage.IpAddress
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

        var clientIp = GetClientIpAddress();
        await Clients.Caller.SendAsync("SetUserIp", clientIp);

        var history = _historyService.GetRecentMessages().Select(m => new
        {
            user = m.User,
            message = m.Message,
            timestamp = m.Timestamp.ToString("HH:mm:ss"),
            isSystem = m.IsSystem,
            ipAddress = m.IpAddress
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

    private string GetClientIpAddress()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null) return "127.0.0.1";

        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
        {
            var ip = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Real-IP", out var realIp) && !string.IsNullOrWhiteSpace(realIp))
        {
            var ip = realIp.ToString().Trim();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }

        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                return remoteIp.MapToIPv4().ToString();
            }
            if (remoteIp.ToString() == "::1")
            {
                return "127.0.0.1";
            }
            return remoteIp.ToString();
        }

        return "127.0.0.1";
    }
}
