using Microsoft.AspNetCore.SignalR;

namespace WebApplicationASP01.Hubs;

public class PersonHub : Hub
{
    // Hub specifically for broadcasting Person CRUD events.
    // Methods like Create, Update, Delete will be triggered from PersonsController.
}
