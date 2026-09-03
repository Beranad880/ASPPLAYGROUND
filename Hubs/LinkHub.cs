using Microsoft.AspNetCore.SignalR;

namespace WebApplicationASP01.Hubs;

public class LinkHub : Hub
{
    // Tento hub slouží pro real-time aktualizace odkazů (Push notifikace)
    // Klienti se pouze připojí a budou poslouchat událost "LinksUpdated"
}
