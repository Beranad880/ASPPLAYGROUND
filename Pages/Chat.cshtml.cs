using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplicationASP01.Pages;

public class ChatModel : PageModel
{
    public string ClientIp { get; private set; } = "127.0.0.1";

    public void OnGet()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
        {
            var ip = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(ip))
            {
                ClientIp = ip;
                return;
            }
        }

        if (Request.Headers.TryGetValue("X-Real-IP", out var realIp) && !string.IsNullOrWhiteSpace(realIp))
        {
            var ip = realIp.ToString().Trim();
            if (!string.IsNullOrEmpty(ip))
            {
                ClientIp = ip;
                return;
            }
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                ClientIp = remoteIp.MapToIPv4().ToString();
            }
            else if (remoteIp.ToString() == "::1")
            {
                ClientIp = "127.0.0.1";
            }
            else
            {
                ClientIp = remoteIp.ToString();
            }
        }
    }
}
