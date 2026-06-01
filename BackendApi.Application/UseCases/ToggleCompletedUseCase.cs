using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Application.UseCases;

public class ToggleCompletedUseCase
{
    private readonly ITodoRepository _repository;

    public ToggleCompletedUseCase(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Todo?> Execute(TodoId id, CancellationToken ct = default)
    {
        var todo = await _repository.GetByIdAsync(id, ct);
        if (todo == null)
            return null;

        var toggled = todo.ToggleCompleted();
        await _repository.UpdateAsync(toggled, ct);
        return toggled;
    }
}
