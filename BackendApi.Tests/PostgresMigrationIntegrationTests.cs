using BackendApi.Storage.Postgres;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace BackendApi.Tests;

public class PostgresMigrationIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private WebApplicationFactory<Program>? _factory;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }
        await _postgresContainer.StopAsync();
    }

    [Fact]
    public async Task Startup_RunsMigrations_TodosTableExists()
    {
        // Arrange
        var connectionString = _postgresContainer.GetConnectionString();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Default", connectionString);
            });

        // Act - force app startup (triggers migration runner)
        using var scope = _factory.Services.CreateScope();

        // Assert - verify todos table exists
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var exists = await connection.QuerySingleAsync<bool>(@"
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_name = 'todos'
            )");

        Assert.True(exists);
    }
}
