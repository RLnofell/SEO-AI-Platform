using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AI_SEO_Ssas_Platform.Services;

public interface ILogCollector
{
    void Initialize();
    Task AddLogAsync(string message);
    List<string> GetLogs();
}

public class LogCollector : ILogCollector
{
    private readonly AsyncLocal<List<string>> _logs = new();
    private readonly IHubContext<AgentHub> _hubContext;

    public LogCollector(IHubContext<AgentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public void Initialize()
    {
        _logs.Value = new List<string>();
    }

    public async Task AddLogAsync(string message)
    {
        _logs.Value?.Add(message);
        System.Console.WriteLine(message);
        await _hubContext.Clients.All.SendAsync("ReceiveLog", message);
    }

    public List<string> GetLogs()
    {
        return _logs.Value?.ToList() ?? new List<string>();
    }

    // Keep static methods for compatibility during transition if needed, 
    // but we will move to DI.
}
