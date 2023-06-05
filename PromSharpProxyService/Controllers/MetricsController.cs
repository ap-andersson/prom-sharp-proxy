using System.Collections.Concurrent;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using PromSharpProxyService.Models;

namespace PromSharpProxyService.Controllers
{
    internal static class MetricsStore
    {
        public static ConcurrentDictionary<string, Dictionary<string, string>> MetricsDict =
            new ConcurrentDictionary<string, Dictionary<string, string>>();
    }

    [ApiController]
    [Route("[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly ILogger<MetricsController> _logger;

        public MetricsController(ILogger<MetricsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Metrics()
        {
            return GenerateMetricsOutput();
        }

        [HttpGet]
        [Route("{collectionName}")]
        public string Metrics(string collectionName)
        {
            return GenerateMetricsOutput(collectionName);
        }

        private string GenerateMetricsOutput(string? collectionName = null)
        {
            var sb = new StringBuilder();

            foreach (var metrics in MetricsStore.MetricsDict)
            {
                var prefix = metrics.Key;

                if(collectionName != null && !string.Equals(prefix, collectionName)) continue;

                foreach (var pair in metrics.Value)
                {
                    sb.Append($"{prefix}_{pair.Key} {pair.Value}\n");
                }
            }

            return sb.ToString();
        }

        [HttpPost]
        public StatusCodeResult SetMetric(SetMetricsModel model)
        {
            MetricsStore.MetricsDict.TryAdd(model.CollectionName, new Dictionary<string, string>());

            var hej = model.MetricsWithValues
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.First().Value);

            MetricsStore.MetricsDict.AddOrUpdate(model.CollectionName, hej, (collectionName, existingMetrics) =>
            {
                foreach (var pair in hej)
                {
                    if (existingMetrics.ContainsKey(pair.Key))
                    {
                        existingMetrics[pair.Key] = pair.Value;
                    }
                    else
                    {
                        existingMetrics.Add(pair.Key, pair.Value);
                    }
                }
                return existingMetrics;
            });

            return StatusCode(200);
        }

        [HttpDelete]
        public StatusCodeResult ClearMetrics(string collection)
        {
            if (MetricsStore.MetricsDict.ContainsKey(collection))
            {
                MetricsStore.MetricsDict.TryRemove(collection, out Dictionary<string, string>? removed);
            }

            return StatusCode(200);
        }
    }
}