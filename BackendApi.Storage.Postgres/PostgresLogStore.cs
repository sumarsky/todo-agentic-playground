using BackendApi.Application;
using BackendApi.Application.Ports;
using BackendApi.Domain;
using Dapper;
using Npgsql;

namespace BackendApi.Storage.Postgres;

public class PostgresLogStore : ILogStore
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresLogStore(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task WriteAsync(LogEntry entry, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO logs (id, timestamp, level, source, message, http_method, http_path, http_status, duration_ms, trace_id, exception_type, exception_message)
            VALUES (@Id, @Timestamp, @Level, @Source, @Message, @HttpMethod, @HttpPath, @HttpStatus, @DurationMs, @TraceId, @ExceptionType, @ExceptionMessage)
            """,
            new { entry.Id, entry.Timestamp, entry.Level, entry.Source, entry.Message, entry.HttpMethod, entry.HttpPath, entry.HttpStatus, entry.DurationMs, entry.TraceId, entry.ExceptionType, entry.ExceptionMessage },
            commandTimeout: 30);
    }

    public async Task<IReadOnlyList<LogEntry>> QueryAsync(LogFilter filter, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);

        var sql = "SELECT id, timestamp, level, source, message, http_method, http_path, http_status, duration_ms, trace_id, exception_type, exception_message FROM logs WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filter.Level))
        {
            sql += " AND level = @Level";
            parameters.Add("Level", filter.Level);
        }

        if (!string.IsNullOrWhiteSpace(filter.Message))
        {
            sql += " AND message ILIKE @Message";
            parameters.Add("Message", $"%{filter.Message}%");
        }

        sql += " ORDER BY timestamp DESC";

        var results = await conn.QueryAsync<LogEntry>(sql, parameters);
        return results.ToList().AsReadOnly();
    }
}
