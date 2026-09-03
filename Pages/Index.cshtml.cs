using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplicationASP01.App;
using WebApplicationASP01.Services;

namespace WebApplicationASP01.Pages;

public class IndexModel : PageModel
{
    private readonly ChatHistoryService _chatHistoryService;

    public IndexModel(ChatHistoryService chatHistoryService)
    {
        _chatHistoryService = chatHistoryService;
    }

    public int ChatMessageCount { get; set; }

    public async Task OnGetAsync()
    {
        var messages = await _chatHistoryService.GetRecentMessagesAsync();
        ChatMessageCount = messages.Count;
    }
}
