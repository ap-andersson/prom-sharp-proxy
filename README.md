# prom-sharp-proxy 

Basic API that stores (in-memory only) submitted metrics and gives them out in prometheus format (kinda).

Useful when you have a device with metrics that Prometheus cannot scrape. Then you can post the metrics to this API and scrape it instead.

Supports time to live for each named metric with default of 60 seconds.

## Examples
Basic usage. All metrics will be outputted in this format: \<CollectionName\>_\<metric\>

### Set metrics
Post to /metrics to set metrics
```
{
    "CollectionName": "metric_name",
    "TimeToLive": 60,
    "MetricsWithValues": [
        {
            "Key": "metric_1",
            "Value": "abc"
        },
        {
            "Key": "metric_dos",
            "Value": "2"
        }
    ]
}
```

### Get specific metrics
```
GET /metrics/metric_name
```

### Get all metrics
```
GET /metrics/
```
