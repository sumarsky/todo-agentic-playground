using BackendApi.Application.Ports;
using BackendApi.Domain;
using Dapper;
using Npgsql;

namespace BackendApi.Storage.Postgres;

public class PostgresTodoRepository : ITodoRepository
{
    private readonly NpgsqlDataSource _dataSource;

    private readonly record struct TodoRow(
        Guid Id,
        string Title,
        bool Completed,
        DateTimeOffset CreatedAt)
    {
        public static TodoRow FromDomain(Todo todo) =>
            new(todo.Id.Value, todo.Title, todo.Completed, todo.CreatedAt);

        public Todo ToDomain() =>
            new(new TodoId(Id), new TodoTitle(Title), Completed, CreatedAt);
    }

    public PostgresTodoRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddAsync(Todo todo, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "INSERT INTO todos (id, title, completed, created_at) VALUES (@Id, @Title, @Completed, @CreatedAt)",
            TodoRow.FromDomain(todo),
            commandTimeout: 30);
    }

    public async Task<Todo?> GetByIdAsync(TodoId id, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        var row = (await conn.QueryAsync<TodoRow>(
            "SELECT id, title, completed, created_at FROM todos WHERE id = @Id",
            new { Id = id.Value }))
            .Cast<TodoRow?>()
            .FirstOrDefault();

        return row?.ToDomain();
    }

    public async Task<IReadOnlyList<Todo>> GetAllAsync(bool? completed = null, string? search = null, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);

        var sql = "SELECT id, title, completed, created_at FROM todos WHERE 1=1";
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

        var results = await conn.QueryAsync<TodoRow>(sql, parameters);
        return results.Select(row => row.ToDomain()).ToList().AsReadOnly();
    }

    public async Task UpdateAsync(Todo todo, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE todos SET title = @Title, completed = @Completed WHERE id = @Id",
            TodoRow.FromDomain(todo),
            commandTimeout: 30);
    }

    public async Task DeleteAsync(TodoId id, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "DELETE FROM todos WHERE id = @Id",
            new { Id = id.Value },
            commandTimeout: 30);
    }

    public async Task DeleteByIdsAsync(IEnumerable<TodoId> ids, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "DELETE FROM todos WHERE id = ANY(@Ids)",
            new { Ids = ids.Select(id => id.Value).ToArray() },
            commandTimeout: 30);
    }
}
