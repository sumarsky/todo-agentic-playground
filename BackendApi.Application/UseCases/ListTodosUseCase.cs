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

    public async Task<IReadOnlyList<Todo>> Execute(bool? completed = null, string? search = null, CancellationToken ct = default)
    {
        return await _repository.GetAllAsync(completed, search, ct);
    }
}
