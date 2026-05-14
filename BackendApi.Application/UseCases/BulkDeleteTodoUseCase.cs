using BackendApi.Application.Ports;

namespace BackendApi.Application.UseCases;

public class BulkDeleteTodoUseCase
{
    private readonly ITodoRepository _repository;

    public BulkDeleteTodoUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public void Execute(IEnumerable<Guid> ids)
    {
        _repository.DeleteByIds(ids);
    }
}
