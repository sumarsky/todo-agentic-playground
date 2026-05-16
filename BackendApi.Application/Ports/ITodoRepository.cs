using BackendApi.Domain;

namespace BackendApi.Application.Ports;

public interface ITodoRepository
{
    Task AddAsync(Todo todo, CancellationToken ct = default);
    Task<Todo?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Todo>> GetAllAsync(bool? completed = null, string? search = null, CancellationToken ct = default);
    Task UpdateAsync(Todo todo, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
