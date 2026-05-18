using BackendApi.Application.Ports;
using BackendApi.Tests.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BackendApi.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ITodoRepository));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<ITodoRepository, FakeTodoRepository>();

            var logStoreDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ILogStore));
            if (logStoreDescriptor != null)
            {
                services.Remove(logStoreDescriptor);
            }

            services.AddSingleton<ILogStore, FakeLogStore>();

            var migrationDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IHostedService)
                      && d.ImplementationType?.FullName == "BackendApi.Storage.Postgres.MigrationRunner");
            if (migrationDescriptor != null)
            {
                services.Remove(migrationDescriptor);
            }
        });
    }
}
