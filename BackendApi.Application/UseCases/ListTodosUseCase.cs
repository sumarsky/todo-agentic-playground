using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Application.UseCases;

public class ListTodosUseCase
{
    private readonly ITodoRepository _repository;

    public ListTodosUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Todo> Execute(bool? completed, string? search)
    {
        var todos = _repository.GetAll().ToList();

        if (completed.HasValue)
        {
            todos = todos.Where(t => t.Completed == completed.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            todos = todos.Where(t => t.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return todos.AsReadOnly();
    }
}
