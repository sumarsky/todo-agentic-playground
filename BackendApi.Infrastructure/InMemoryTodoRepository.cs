using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Infrastructure;

public class InMemoryTodoRepository : ITodoRepository
{
    private readonly Dictionary<Guid, Todo> _todos = new();
    private readonly object _lock = new();

    public void Add(Todo todo)
    {
        lock (_lock)
        {
            _todos[todo.Id] = todo;
        }
    }

    public Todo? GetById(Guid id)
    {
        lock (_lock)
        {
            _todos.TryGetValue(id, out var todo);
            return todo;
        }
    }

    public IReadOnlyList<Todo> GetAll()
    {
        lock (_lock)
        {
            return _todos.Values.ToList().AsReadOnly();
        }
    }

    public void Update(Todo todo)
    {
        lock (_lock)
        {
            if (_todos.ContainsKey(todo.Id))
            {
                _todos[todo.Id] = todo;
            }
        }
    }

    public void Delete(Guid id)
    {
        lock (_lock)
        {
            _todos.Remove(id);
        }
    }

    public void DeleteByIds(IEnumerable<Guid> ids)
    {
        lock (_lock)
        {
            foreach (var id in ids)
            {
                _todos.Remove(id);
            }
        }
    }
}
