using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Application.UseCases;

public class UpdateTitleUseCase
{
    private readonly ITodoRepository _repository;

    public UpdateTitleUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public Todo? Execute(Guid id, string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty or null", nameof(newTitle));

        var todo = _repository.GetById(id);
        if (todo == null)
            return null;

        todo.UpdateTitle(newTitle);
        _repository.Update(todo);
        return todo;
    }
}
