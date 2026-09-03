using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplicationASP01.Models;
using WebApplicationASP01.Services;

namespace WebApplicationASP01.Pages;

public class CheckModel : PageModel
{
    private readonly SystemCheckService _checkService;

    public SystemCheckResponse CheckResult { get; private set; } = new();

    public CheckModel(SystemCheckService checkService)
    {
        _checkService = checkService;
    }

    public async Task OnGetAsync()
    {
        CheckResult = await _checkService.PerformCheckAsync();
    }
}
