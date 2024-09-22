using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Godot;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Mystic.Shared.Utils.Monitoring;

public static class Instrumentation
{
    private const string Prefix = "mystic_";

    private static readonly ActivitySource Source;
    private static readonly Meter Meter;

    static Instrumentation()
    {
        var serviceName = OS.GetEnvironment("OTEL_SERVICE_NAME");
        var serviceVersion = OS.GetEnvironment("OTEL_SERVICE_VERSION");

        if (serviceName == "")
            return;

        GD.Print("Enabling OpenTelemetry instrumentation");

        var resource = ResourceBuilder.CreateDefault();

        Sdk.CreateTracerProviderBuilder()
            .AddSource(serviceName)
            .SetResourceBuilder(resource)
            .AddHttpClientInstrumentation()
            .AddOtlpExporter()
            .Build();

        Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resource)
            .AddPrometheusHttpListener()
            .AddProcessInstrumentation() // https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/Instrumentation.Process-0.5.0-beta.6/src/OpenTelemetry.Instrumentation.Process/README.md#metrics
            .AddRuntimeInstrumentation() // https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/Instrumentation.Runtime-1.9.0/src/OpenTelemetry.Instrumentation.Runtime/README.md#metrics
            .AddHttpClientInstrumentation() // https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/Instrumentation.Http-1.9.0/src/OpenTelemetry.Instrumentation.Http/README.md#list-of-metrics-produced
            .AddMeter(serviceName)
            .AddOtlpExporter((_, metricOptions) =>
            {
                metricOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
            })
            .Build();

        LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(logging =>
            {
                logging.SetResourceBuilder(resource).AddOtlpExporter();
            });
        });

        Source = new ActivitySource(serviceName, serviceVersion);
        Meter = new Meter(serviceName, serviceVersion);

        // Most of the process.runtime.dotnet.gc.* metrics are only available after the GC finished at least one collection.
        GC.Collect(1);
    }

    public static Activity Measure(string name) => Source.StartActivity(Prefix + name);

    public static Counter<T> Counter<T>(string name)
        where T : struct => Meter.CreateCounter<T>(Prefix + name);

    public static UpDownCounter<T> UpDownCounter<T>(string name)
        where T : struct => Meter.CreateUpDownCounter<T>(Prefix + name);

    public static ObservableCounter<T> ObservableCounter<T>(string name, Func<T> measure)
        where T : struct => Meter.CreateObservableCounter(Prefix + name, measure);
}

