using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Application.UseCases;

public class ListTodosUseCase
{
    private readonly ITodoRepository _repository;

    public ListTodosUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Todo>> Execute(bool? completed, TodoTitleSearch search, CancellationToken ct = default)
    {
        return await _repository.GetAllAsync(completed, search.Value, ct);
    }
}
