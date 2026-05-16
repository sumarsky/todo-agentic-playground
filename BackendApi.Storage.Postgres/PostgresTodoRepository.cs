using BackendApi.Application.Ports;
using BackendApi.Domain;
using Dapper;
using Npgsql;

namespace BackendApi.Storage.Postgres;

public class PostgresTodoRepository : ITodoRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresTodoRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddAsync(Todo todo, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "INSERT INTO todos (id, title, completed, created_at) VALUES (@Id, @Title, @Completed, @CreatedAt)",
            new { todo.Id, todo.Title, todo.Completed, todo.CreatedAt },
            commandTimeout: 30);
    }

    public async Task<Todo?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<Todo>(
            "SELECT id, title, completed, created_at AS CreatedAt FROM todos WHERE id = @Id",
            new { Id = id });
    }

    public async Task<IReadOnlyList<Todo>> GetAllAsync(bool? completed = null, string? search = null, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);

        var sql = "SELECT id, title, completed, created_at AS CreatedAt FROM todos WHERE 1=1";
        var parameters = new DynamicParameters();

        if (completed.HasValue)
        {
            sql += " AND completed = @Completed";
            parameters.Add("Completed", completed.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND title ILIKE @Search";
            parameters.Add("Search", $"%{search}%");
        }

        var results = await conn.QueryAsync<Todo>(sql, parameters);
        return results.ToList().AsReadOnly();
    }

    public async Task UpdateAsync(Todo todo, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE todos SET title = @Title, completed = @Completed WHERE id = @Id",
            new { todo.Id, todo.Title, todo.Completed },
            commandTimeout: 30);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "DELETE FROM todos WHERE id = @Id",
            new { Id = id },
            commandTimeout: 30);
    }

    public async Task DeleteByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
            return;

        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "DELETE FROM todos WHERE id = ANY(@Ids)",
            new { Ids = idList },
            commandTimeout: 30);
    }
}
