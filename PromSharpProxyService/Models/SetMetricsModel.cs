using System.Text.Json.Serialization;

namespace PromSharpProxyService.Models
{
    public class SetMetricsModel
    {

        public string CollectionName { get; set; }

        public List<MetricModel> MetricsWithValues { get; set; }
    }

    public class MetricModel
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
