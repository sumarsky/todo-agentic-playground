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

    public async Task<Todo> Execute(TodoTitle title, CancellationToken ct = default)
    {
        var todo = new Todo(TodoId.New(), title);
        await _repository.AddAsync(todo, ct);
        return todo;
    }
}
