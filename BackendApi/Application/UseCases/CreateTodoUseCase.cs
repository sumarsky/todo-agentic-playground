using BackendApi.Domain;

namespace BackendApi.Application.UseCases;

public class CreateTodoUseCase
{
    private readonly ITodoRepository _repository;

    public CreateTodoUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public Todo Execute(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty or null", nameof(title));

        var todo = new Todo(Guid.NewGuid(), title);
        _repository.Add(todo);
        return todo;
    }
}
