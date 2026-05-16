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

    public async Task<Todo?> Execute(Guid id, string newTitle, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty or null", nameof(newTitle));

        var todo = await _repository.GetByIdAsync(id, ct);
        if (todo == null)
            return null;

        var updated = todo.WithTitle(newTitle);
        await _repository.UpdateAsync(updated, ct);
        return updated;
    }
}
