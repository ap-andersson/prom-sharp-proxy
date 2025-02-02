using PromSharpProxyService.Controllers;
using System.Collections.Concurrent;

namespace PromSharpProxyService;

internal static class MetricsStore
{
    public static ConcurrentDictionary<string, Metric> MetricsDict =
        new ConcurrentDictionary<string, Metric>();
}
