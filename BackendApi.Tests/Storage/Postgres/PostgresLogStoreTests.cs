using BackendApi.Application;
using BackendApi.Domain;
using BackendApi.Storage.Postgres;
using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;

namespace BackendApi.Tests.Storage.Postgres;

public class PostgresLogStoreTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private NpgsqlDataSource? _dataSource;
    private PostgresLogStore? _store;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();
        _dataSource = NpgsqlDataSource.Create(connectionString);

        await using var conn = await _dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS logs (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                timestamp TIMESTAMPTZ NOT NULL DEFAULT now(),
                level VARCHAR(10) NOT NULL,
                source VARCHAR(100) NOT NULL,
                message TEXT NOT NULL,
                http_method VARCHAR(10),
                http_path VARCHAR(500),
                http_status INT,
                duration_ms DOUBLE PRECISION,
                trace_id VARCHAR(50),
                exception_type VARCHAR(200),
                exception_message TEXT
            )");

        _store = new PostgresLogStore(_dataSource);
    }

    public async Task DisposeAsync()
    {
        if (_dataSource != null)
        {
            await _dataSource.DisposeAsync();
        }
        await _postgresContainer.StopAsync();
    }

    [Fact]
    public async Task WriteAsync_InsertsLogEntry_CanQueryBack()
    {
        // Arrange
        var entry = new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "info", "test", "Test log message");

        // Act
        await _store!.WriteAsync(entry);
        var results = await _store.QueryAsync(new LogFilter());

        // Assert
        var result = Assert.Single(results);
        Assert.Equal(entry.Id, result.Id);
        Assert.Equal(entry.Level, result.Level);
        Assert.Equal(entry.Source, result.Source);
        Assert.Equal(entry.Message, result.Message);
    }

    [Fact]
    public async Task QueryAsync_NoFilters_ReturnsAllEntriesOrderedByTimestampDesc()
    {
        // Arrange
        var entry1 = new LogEntry(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-2), "info", "test", "First");
        var entry2 = new LogEntry(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-1), "warning", "test", "Second");
        var entry3 = new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "error", "test", "Third");

        await _store!.WriteAsync(entry1);
        await _store.WriteAsync(entry2);
        await _store.WriteAsync(entry3);

        // Act
        var results = await _store.QueryAsync(new LogFilter());

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("Third", results[0].Message);
        Assert.Equal("Second", results[1].Message);
        Assert.Equal("First", results[2].Message);
    }

    [Fact]
    public async Task QueryAsync_FilterByLevel_ReturnsMatchingEntries()
    {
        // Arrange
        await _store!.WriteAsync(new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "info", "test", "Info message"));
        await _store.WriteAsync(new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "warning", "test", "Warning message"));
        await _store.WriteAsync(new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "error", "test", "Error message"));

        // Act
        var results = await _store.QueryAsync(new LogFilter { Level = "error" });

        // Assert
        Assert.Single(results);
        Assert.Equal("error", results[0].Level);
        Assert.Equal("Error message", results[0].Message);
    }

    [Fact]
    public async Task QueryAsync_FilterByMessage_PartialMatch()
    {
        // Arrange
        await _store!.WriteAsync(new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "info", "test", "User login successful"));
        await _store.WriteAsync(new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "warning", "test", "User session expiring"));
        await _store.WriteAsync(new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "error", "test", "Database connection failed"));

        // Act
        var results = await _store.QueryAsync(new LogFilter { Message = "User" });

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.Contains("User", e.Message));
    }

    [Fact]
    public async Task QueryAsync_CombinedFilters_ReturnsCorrectResults()
    {
        // Arrange
        await _store!.WriteAsync(new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "info", "auth", "User login"));
        await _store.WriteAsync(new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "error", "auth", "Auth failed"));
        await _store.WriteAsync(new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "info", "db", "Query executed"));

        // Act
        var results = await _store.QueryAsync(new LogFilter { Level = "info", Message = "User" });

        // Assert
        Assert.Single(results);
        Assert.Equal("info", results[0].Level);
        Assert.Contains("User", results[0].Message);
    }

    [Fact]
    public async Task QueryAsync_NoMatchingEntries_ReturnsEmpty()
    {
        // Arrange
        await _store!.WriteAsync(new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "info", "test", "Test message"));

        // Act
        var results = await _store.QueryAsync(new LogFilter { Level = "error" });

        // Assert
        Assert.Empty(results);
    }
}
