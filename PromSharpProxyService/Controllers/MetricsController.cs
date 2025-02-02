using System.Text;
using Microsoft.AspNetCore.Mvc;
using PromSharpProxyService.Models;

namespace PromSharpProxyService.Controllers;

internal class Metric
{
    private uint timeToLiveInSeconds;

    public DateTime Expires { get; private set; }
    public Dictionary<string, string> Metrics { get; set; }

    public Metric(uint? timeToLive)
    {
        timeToLiveInSeconds = timeToLive ?? 10 * 60; // Default is 10 minutes

        Metrics = new Dictionary<string, string>();
        Expires = DateTime.Now.AddSeconds(timeToLiveInSeconds);
    }

    public void UpdateExpire(uint? timeToLive)
    {
        timeToLiveInSeconds = timeToLive ?? timeToLiveInSeconds;
        Expires = DateTime.Now.AddSeconds(timeToLiveInSeconds);
    }
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
            var metricName = metrics.Key;

            // Remove expired metrics and skip
            if (metrics.Value.Expires < DateTime.Now)
            {
                MetricsStore.MetricsDict.TryRemove(metrics.Key, out _);
                continue;
            }

            if (collectionName != null && !string.Equals(metricName, collectionName)) continue;

            foreach (var pair in metrics.Value.Metrics)
            {
                sb.Append($"{metricName}_{pair.Key} {pair.Value}\n");
            }
        }

        return sb.ToString();
    }

    [HttpPost]
    public StatusCodeResult SetMetric(SetMetricsModel model)
    {
        // Validation
        if (model == null) return StatusCode(StatusCodes.Status400BadRequest);
        if (model.CollectionName == null) return StatusCode(StatusCodes.Status400BadRequest);
        if (model.MetricsWithValues == null || !model.MetricsWithValues.Any()) return StatusCode(StatusCodes.Status400BadRequest);

        // Should not be needed, right?
        //MetricsStore.MetricsDict.TryAdd(model.CollectionName, new Metric(model.TimeToLive));

        // Group the metrics by key and use first best value, no duplicates allowed
        var metricsByKeyAndFirstValue = model.MetricsWithValues
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.First().Value);

        var inputAsMetric = new Metric(model.TimeToLive)
        {
            Metrics = metricsByKeyAndFirstValue
        };

        MetricsStore.MetricsDict.AddOrUpdate(model.CollectionName, inputAsMetric, (collectionName, metric) =>
        {
            foreach (var pair in metricsByKeyAndFirstValue)
            {
                if (metric.Metrics.ContainsKey(pair.Key))
                {
                    metric.Metrics[pair.Key] = pair.Value;
                }
                else
                {
                    metric.Metrics.Add(pair.Key, pair.Value);
                }
            }

            metric.UpdateExpire(model.TimeToLive);

            return metric;
        });

        return StatusCode(StatusCodes.Status200OK);
    }

    [HttpDelete]
    public StatusCodeResult ClearMetrics(string collection)
    {
        if (MetricsStore.MetricsDict.ContainsKey(collection))
        {
            MetricsStore.MetricsDict.TryRemove(collection, out Metric? removed);
        }

        return StatusCode(200);
    }
}