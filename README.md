# prom-sharp-proxy 

Basic API that stores (in-memory only) submitted metrics and gives them out in prometheus format (kinda).

Useful when you have a device with metrics that Prometheus cannot scrape. Then you can post the metrics to this API and scrape it instead.

Supports time to live for each named metric with default of 60 seconds.
