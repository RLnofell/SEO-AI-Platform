using Microsoft.AspNetCore.SignalR;

namespace AI_SEO_Ssas_Platform.Services;

public class AgentHub : Hub
{
    public async Task SendLog(string message)
    {
        await Clients.All.SendAsync("ReceiveLog", message);
    }
}
