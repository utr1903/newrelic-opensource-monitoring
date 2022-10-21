using dotnet_second.Commons;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

GetEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);

// Http Client
builder.Services.AddHttpClient();

// Open Telemetry
var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(Constants.OTEL_SERVICE_NAME)
    //.AddAttributes(new Dictionary<string, object> {
    //    { "environment", "production" }
    //})
    .AddTelemetrySdk();

builder.Services.AddOpenTelemetryTracing(b =>
{
    // Decorate our service name so we can find it when we search traces
    b.SetResourceBuilder(resourceBuilder);

    // Receive traces from built-in sources
    b.AddHttpClientInstrumentation();
    b.AddAspNetCoreInstrumentation(options =>
    {
        options.RecordException = true;
    });

    // Use the OTLP exporter
    b.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri($"{Constants.OTEL_EXPORTER_OTLP_ENDPOINT}");
    });

    // Receive traces from our own custom sources
    b.AddSource(Constants.OTEL_SERVICE_NAME);
});

// Controllers
builder.Services.AddControllers();

builder.Services.AddMetricServer(options =>
{
    options.Hostname = Constants.POD_NAME;
    options.Port = 5000;
    options.Url = "/metrics";
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run("http://*:8080");

void GetEnvironmentVariables()
{
    Console.WriteLine("Getting environment variables...");

    var podName = Environment.GetEnvironmentVariable("POD_NAME");
    if (string.IsNullOrEmpty(podName))
    {
        Console.WriteLine("[POD_NAME] is not provided");
        Environment.Exit(1);
    }
    Constants.POD_NAME = podName;

    var otelServiceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
    if (string.IsNullOrEmpty(otelServiceName))
    {
        Console.WriteLine("[OTEL_SERVICE_NAME] is not provided");
        Environment.Exit(1);
    }
    Constants.OTEL_SERVICE_NAME = otelServiceName;

    var otelExporterOtlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
    if (string.IsNullOrEmpty(otelExporterOtlpEndpoint))
    {
        Console.WriteLine("[OTEL_EXPORTER_OTLP_ENDPOINT] is not provided");
        Environment.Exit(1);
    }
    Constants.OTEL_EXPORTER_OTLP_ENDPOINT = otelExporterOtlpEndpoint;
}
