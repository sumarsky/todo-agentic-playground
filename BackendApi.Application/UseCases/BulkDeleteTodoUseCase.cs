using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Application.UseCases;

public class BulkDeleteTodoUseCase
{
    private readonly ITodoRepository _repository;

    public BulkDeleteTodoUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public Task Execute(IEnumerable<TodoId> ids, CancellationToken ct = default)
    {
        return _repository.DeleteByIdsAsync(ids, ct);
    }
}
