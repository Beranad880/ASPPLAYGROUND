using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplicationASP01.App;

namespace WebApplicationASP01.Pages;

public class PersonsModel : PageModel
{
    private readonly PersonService _personService;

    public PersonsModel(PersonService personService)
    {
        _personService = personService;
    }

    public List<Person> Persons { get; set; } = new();

    public async Task OnGetAsync()
    {
        Persons = await _personService.GetAllAsync();
    }
}
