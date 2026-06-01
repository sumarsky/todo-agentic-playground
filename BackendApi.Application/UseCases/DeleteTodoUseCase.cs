using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Application.UseCases;

public class DeleteTodoUseCase
{
    private readonly ITodoRepository _repository;

    public DeleteTodoUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public Task Execute(Guid id, CancellationToken ct = default)
    {
        return _repository.DeleteAsync(new TodoId(id), ct);
    }
}
