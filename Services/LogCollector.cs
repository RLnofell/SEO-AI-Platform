using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AI_SEO_Ssas_Platform.Services;

public static class LogCollector
{
    private static readonly AsyncLocal<List<string>> _logs = new();

    public static void Initialize()
    {
        _logs.Value = new List<string>();
    }

    public static void AddLog(string message)
    {
        _logs.Value?.Add(message);
        System.Console.WriteLine(message);
    }

    public static List<string> GetLogs()
    {
        return _logs.Value?.ToList() ?? new List<string>();
    }
}
