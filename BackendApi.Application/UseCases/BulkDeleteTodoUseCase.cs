using BackendApi.Application.Ports;

namespace BackendApi.Application.UseCases;

public class BulkDeleteTodoUseCase
{
    private readonly ITodoRepository _repository;

    public BulkDeleteTodoUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public Task Execute(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return _repository.DeleteByIdsAsync(ids, ct);
    }
}
