using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Application.UseCases;

public class CreateTodoUseCase
{
    private readonly ITodoRepository _repository;

    public CreateTodoUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Todo> Execute(string title, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty or null", nameof(title));

        var todo = new Todo(Guid.NewGuid(), title);
        await _repository.AddAsync(todo, ct);
        return todo;
    }
}
