using System;
using dotnet_second.Commons;
using Newtonsoft.Json;

namespace dotnet_second.Logging;

public class CustomLog
{
    // Code specific properties
    [JsonProperty("className")]
    public string ClassName { get; set; }

    [JsonProperty("methodName")]
    public string MethodName { get; set; }

    [JsonProperty("logLevel")]
    public string LogLevel { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("exception")]
    public string Exception { get; set; }

    [JsonProperty("stackTrace")]
    public string StackTrace { get; set; }

    // Trace specific properties
    [JsonProperty("hostname")]
    public string HostName { get; set; } = Constants.POD_NAME;

    [JsonProperty("service.name")]
    public string ServiceName { get; set; } = Constants.OTEL_SERVICE_NAME;

    [JsonProperty("trace.id")]
    public string TraceId { get; set; }

    [JsonProperty("span.id")]
    public string SpanId { get; set; }
}

