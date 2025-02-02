using System.ComponentModel.DataAnnotations;

namespace PromSharpProxyService.Models
{
    public class SetMetricsModel
    {
        // Name used to identify a set of values
        public string? CollectionName { get; set; }

        // After this amount of time without updates, they will be cleared
        public uint? TimeToLive { get; set; }

        public List<MetricModel>? MetricsWithValues { get; set; }
    }

    public class MetricModel
    {
        public required string Key { get; set; }

        public required string Value { get; set; }
    }
}
