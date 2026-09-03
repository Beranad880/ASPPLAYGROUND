using Microsoft.AspNetCore.SignalR;

namespace WebApplicationASP01.Hubs;

public class NotesHub : Hub
{
    // Real-time události pro poznámky
    // Klienti mohou naslouchat na "NoteCreated", "NoteUpdated", "NoteDeleted"
}
