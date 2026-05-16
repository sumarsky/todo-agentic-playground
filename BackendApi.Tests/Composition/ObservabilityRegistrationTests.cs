using BackendApi.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace BackendApi.Tests.Composition;

public class ObservabilityRegistrationTests
{
    [Fact]
    public void AddObservability_RegistersOTelServices()
    {
        var builder = Host.CreateEmptyApplicationBuilder(new());
        builder.Configuration.AddInMemoryCollection(new KeyValuePair<string, string?>[]
        {
            new("OTEL_SERVICE_NAME", "BackendApi")
        }!);

        builder.AddObservability();

        using var provider = builder.Services.BuildServiceProvider();

        var loggerProvider = provider.GetService<LoggerProvider>();
        var tracerProvider = provider.GetService<TracerProvider>();
        var meterProvider = provider.GetService<MeterProvider>();

        Assert.NotNull(loggerProvider);
        Assert.NotNull(tracerProvider);
        Assert.NotNull(meterProvider);
    }
}
