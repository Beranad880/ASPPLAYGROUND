using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplicationASP01.App;
using WebApplicationASP01.Services;

namespace WebApplicationASP01.Pages;

public class IndexModel : PageModel
{
    private readonly PersonService _personService;
    private readonly ChatHistoryService _chatHistoryService;

    public IndexModel(PersonService personService, ChatHistoryService chatHistoryService)
    {
        _personService = personService;
        _chatHistoryService = chatHistoryService;
    }

    public int PersonCount { get; set; }
    public int ChatMessageCount { get; set; }

    public async Task OnGetAsync()
    {
        var persons = await _personService.GetAllAsync();
        PersonCount = persons.Count;
        var messages = await _chatHistoryService.GetRecentMessagesAsync();
        ChatMessageCount = messages.Count;
    }
}
