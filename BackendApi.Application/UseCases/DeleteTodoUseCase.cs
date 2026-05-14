using BackendApi.Application.Ports;

namespace BackendApi.Application.UseCases;

public class DeleteTodoUseCase
{
    private readonly ITodoRepository _repository;

    public DeleteTodoUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public void Execute(Guid id)
    {
        _repository.Delete(id);
    }
}
