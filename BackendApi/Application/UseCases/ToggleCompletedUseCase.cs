using BackendApi.Domain;

namespace BackendApi.Application.UseCases;

public class ToggleCompletedUseCase
{
    private readonly ITodoRepository _repository;

    public ToggleCompletedUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public Todo? Execute(Guid id)
    {
        var todo = _repository.GetById(id);
        if (todo == null)
            return null;

        todo.ToggleCompleted();
        _repository.Update(todo);
        return todo;
    }
}
