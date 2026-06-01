using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Tests.TestDoubles;

public class FakeTodoRepository : ITodoRepository
{
    private readonly Dictionary<TodoId, Todo> _todos = new();
    private readonly object _lock = new();

    public Task AddAsync(Todo todo, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _todos[todo.Id] = todo;
        }
        return Task.CompletedTask;
    }

    public Task<Todo?> GetByIdAsync(TodoId id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _todos.TryGetValue(id, out var todo);
            return Task.FromResult(todo);
        }
    }

    public Task<IReadOnlyList<Todo>> GetAllAsync(bool? completed = null, string? search = null, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var todos = _todos.Values.ToList();

            if (completed.HasValue)
            {
                todos = todos.Where(t => t.Completed == completed.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                todos = todos.Where(t => t.Title.Value.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Task.FromResult<IReadOnlyList<Todo>>(todos.AsReadOnly());
        }
    }

    public Task UpdateAsync(Todo todo, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_todos.ContainsKey(todo.Id))
            {
                _todos[todo.Id] = todo;
            }
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TodoId id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _todos.Remove(id);
        }
        return Task.CompletedTask;
    }

    public Task DeleteByIdsAsync(IEnumerable<TodoId> ids, CancellationToken ct = default)
    {
        lock (_lock)
        {
            foreach (var id in ids)
            {
                _todos.Remove(id);
            }
        }
        return Task.CompletedTask;
    }
}
