using Microsoft.Extensions.Hosting;
using Npgsql;

namespace BackendApi.Storage.Postgres;

public class MigrationRunner : IHostedService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IHostEnvironment _environment;

    public MigrationRunner(NpgsqlDataSource dataSource, IHostEnvironment environment)
    {
        _dataSource = dataSource;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var migrationsFolder = Path.Combine(_environment.ContentRootPath, "Migrations");
        if (!Directory.Exists(migrationsFolder))
            return;

        var sqlFiles = Directory.GetFiles(migrationsFolder, "*.sql")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        foreach (var file in sqlFiles)
        {
            var sql = await File.ReadAllTextAsync(file, cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
