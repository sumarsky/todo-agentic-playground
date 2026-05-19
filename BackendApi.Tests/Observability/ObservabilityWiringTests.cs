using BackendApi.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xunit;

namespace BackendApi.Tests.Observability;

public class ObservabilityWiringTests
{
    [Fact]
    public void AddObservability_RegistersTracerProvider()
    {
        var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var tracerProvider = scope.ServiceProvider.GetService<TracerProvider>();
        Assert.NotNull(tracerProvider);
    }

    [Fact]
    public void AddObservability_RegistersMeterProvider()
    {
        var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var meterProvider = scope.ServiceProvider.GetService<MeterProvider>();
        Assert.NotNull(meterProvider);
    }

    [Fact]
    public void AddObservability_RegistersOpenTelemetryLoggerProvider()
    {
        var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var loggerProviders = scope.ServiceProvider.GetServices<ILoggerProvider>();
        var hasOpenTelemetryProvider = loggerProviders.Any(p =>
            p.GetType().Name.Contains("OpenTelemetry"));
        Assert.True(hasOpenTelemetryProvider);
    }
}
