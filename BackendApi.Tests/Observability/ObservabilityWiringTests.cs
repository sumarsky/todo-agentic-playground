using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace BackendApi.Tests.Observability;

public class ObservabilityWiringTests
{
    [Fact]
    public void AddObservability_RegistersTracerProvider()
    {
        var factory = new TestableWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var tracerProvider = scope.ServiceProvider.GetService<TracerProvider>();
        Assert.NotNull(tracerProvider);
    }

    [Fact]
    public void AddObservability_RegistersMeterProvider()
    {
        var factory = new TestableWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var meterProvider = scope.ServiceProvider.GetService<MeterProvider>();
        Assert.NotNull(meterProvider);
    }

    [Fact]
    public void AddObservability_RegistersOpenTelemetryLoggerProvider()
    {
        var factory = new TestableWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var loggerProviders = scope.ServiceProvider.GetServices<ILoggerProvider>();
        var hasOpenTelemetryProvider = loggerProviders.Any(p =>
            p.GetType().Name.Contains("OpenTelemetry"));
        Assert.True(hasOpenTelemetryProvider);
    }

    private class TestableWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IConfiguration>(
                    new ConfigurationBuilder()
                        .AddInMemoryCollection(new KeyValuePair<string, string?>[]
                        {
                            new("ConnectionStrings:Default", "Host=localhost;Database=test;Username=test;Password=test")
                        }!)
                        .Build());
            });
        }
    }
}
